namespace MusicPlayerBackend.Host.Controllers

open System
open System.IO
open System.Net.Mime
open System.Threading
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.EntityFrameworkCore

open MusicPlayerBackend.Common
open MusicPlayerBackend.Data
open MusicPlayerBackend.Data.Entities
open MusicPlayerBackend.Data.Repositories
open MusicPlayerBackend.Host
open MusicPlayerBackend.Services
open MusicPlayerBackend.TransferObjects
open MusicPlayerBackend.TransferObjects.Track

type TrackListItem = MusicPlayerBackend.TransferObjects.Track.TrackListItem
type TrackVisibility = MusicPlayerBackend.Data.Entities.TrackVisibility

[<ApiController>]
[<Consumes(MediaTypeNames.Application.Json)>]
[<Produces(MediaTypeNames.Application.Json)>]
[<ProducesResponseType(StatusCodes.Status400BadRequest)>]
[<ProducesResponseType(typeof<UnauthorizedResponse>, StatusCodes.Status401Unauthorized)>]
[<Authorize>]
[<Route("[controller]")>]
type TrackController(trackRepository: ITrackRepository,
                     s3Service: IS3Service,
                     unitOfWork: IUnitOfWork,
                     userProvider: UserProvider) =
    inherit ControllerBase()

    /// <summary>
    ///     Gets track list.
    /// </summary>
    [<HttpGet(Name = "GetTracks")>]
    [<ProducesResponseType(typeof<TrackListResponse>, StatusCodes.Status200OK)>]
    member this.List([<FromQuery>] request: PlaylistListRequest, ct: CancellationToken) = task {
        let! userId = userProvider.GetUserId()
        let! tracks =
            query {
                for t in trackRepository.QueryAll() do
                    skip(request.Page * request.PageSize)
                    take(request.PageSize)
                    select(TrackListItem(
                        Author = t.Author,
                        CoverUri = t.CoverUri,
                        Name = t.Name,
                        TrackUri = (if t.OwnerUserId = userId || t.Visibility = TrackVisibility.Visible then t.TrackUri else null),
                        Visibility = (int t.Visibility |> enum))
                    )

            } |> _.ToArrayAsync(ct)
        let! trackCount = trackRepository.CountAsync((fun t -> t.OwnerUserId = userId), ct)
        return this.Ok(TrackListResponse(Items = tracks, Count = trackCount))
    }

    /// <summary>
    ///     Gets track by Id.
    /// </summary>
    [<HttpGet("{id:guid}", Name = "GetTrack")>]
    [<ProducesResponseType(typeof<TrackResponse>, StatusCodes.Status200OK)>]
    member this.Get(id: Guid, ct: CancellationToken) = task {
        let! userId = userProvider.GetUserId()
        let! track = trackRepository.GetByIdIfVisibleOrDefault(id, userId, ct)
        if track = null then
            return this.NotFound() :> IActionResult
        else
            let track = TrackResponse(
                Id = track.Id,
                CoverUri = track.CoverUri,
                IsAvailable = (track.OwnerUserId = userId || track.Visibility = TrackVisibility.Visible),
                TrackUri = track.TrackUri,
                Visibility = ucast track.Visibility,
                Name = track.Name,
                Author = track.Author
            )
            return this.Ok(track) :> IActionResult
    }

    /// <summary>
    ///     Updates track's visibility level, cover.
    /// </summary>
    [<HttpPut("{id:guid}", Name = "UpdateTrack")>]
    [<Consumes(MediaTypeNames.Application.FormUrlEncoded)>]
    member this.Update(id: Guid, [<FromForm>] request: TrackUpdateRequest, ct: CancellationToken) = task {
        let! userId = userProvider.GetUserId()
        let! track = trackRepository.GetByIdIfOwnerOrDefault(userId, id, ct)
        if track = null then
            return this.NotFound() :> IActionResult
        elif request.Visibility.HasValue then
            track.Visibility <- ucast request.Visibility.Value
            return this.NoContent() :> IActionResult
        elif request.Cover <> null then
            if request.RemoveCover then
                return this.BadRequest(UpdateTrackErrorResponse(Error = $"{nameof(request.RemoveCover)} is true")) :> IActionResult
            else
                let! uploadedCoverUri = s3Service.TryUploadFileStream("covers", Guid.NewGuid().ToString() + "_" + track.Name, request.Cover.OpenReadStream(), Path.GetExtension(request.Cover.FileName), ct)
                if uploadedCoverUri = null then
                    return this.BadRequest("Failed to upload cover") :> IActionResult
                else
                    track.CoverUri <- uploadedCoverUri
                    if request.RemoveCover then
                        track.CoverUri <- null
                        trackRepository.Save(track)
                        do! unitOfWork.SaveChangesAsync(ct)
                        return this.NoContent() :> IActionResult
                    else
                        return this.NoContent() :> IActionResult
        else
            return this.NoContent() :> IActionResult
    }

    /// <summary>
    ///     Deletes track.
    /// </summary>
    [<HttpDelete("{id:guid}", Name = "DeleteTrack")>]
    member this.Delete(id: Guid, ct: CancellationToken) = task {
        let! userId = userProvider.GetUserId()
        let! track = trackRepository.GetByIdIfOwnerOrDefault(id, userId, ct)
        if track = null then
            return this.NotFound() :> IActionResult
        else
            trackRepository.Delete(track)
            do! unitOfWork.SaveChangesAsync(ct)
            return this.NoContent() :> IActionResult
    }

    /// <summary>
    ///     Uploads track.
    /// </summary>
    [<HttpPost(Name = "UploadTrack")>]
    [<Consumes(MediaTypeNames.Multipart.FormData)>]
    [<ProducesResponseType(typeof<TrackResponse>, StatusCodes.Status200OK)>]
    member this.Upload([<FromForm>] request: TrackCreateRequest, ct: CancellationToken) = task {
        let! uploadedTrackUri = s3Service.TryUploadFileStream(
            "tracks",
            Guid.NewGuid().ToString() + "_" + request.Name,
            request.Track.OpenReadStream(),
            Path.GetExtension(request.Track.FileName),
            ct)

        if uploadedTrackUri = null then
            return this.BadRequest("Failed to upload track") :> IActionResult
        else
            let mutable coverUri = null
            if request.Cover <> null then
                let! uploadedCoverUri = s3Service.TryUploadFileStream(
                    "covers", Guid.NewGuid().ToString() + "_" + request.Name,
                    request.Cover.OpenReadStream(),
                    Path.GetExtension(request.Cover.FileName),
                    ct)

                if uploadedCoverUri = null then
                    return this.BadRequest("Failed to upload cover") :> IActionResult
                else
                    coverUri <- uploadedCoverUri

                    let! ownerUserId = userProvider.GetUserId()
                    let track = Track(
                        CoverUri = coverUri,
                        Name = request.Name,
                        Author = request.Author,
                        Visibility = ucast request.Visibility,
                        TrackUri = uploadedTrackUri,
                        OwnerUserId = ownerUserId)

                    trackRepository.Save(track)
                    do! unitOfWork.SaveChangesAsync(ct)
                    let track = TrackResponse(
                        Id = track.Id,
                        CoverUri = track.CoverUri,
                        IsAvailable = true,
                        TrackUri = track.TrackUri,
                        Visibility = ucast track.Visibility,
                        Name = track.Name,
                        Author = track.Author
                    )
                    return this.Ok(track) :> IActionResult
            else
                return this.NoContent() :> IActionResult
    }


