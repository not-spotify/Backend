namespace MusicPlayerBackend.Host.Controllers

open System
open System.IO
open System.Net.Mime
open System.Threading
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc

open MusicPlayerBackend.Common
open MusicPlayerBackend.Host
open MusicPlayerBackend.Host.Services
open MusicPlayerBackend.Persistence
open MusicPlayerBackend.Persistence.Entities
open MusicPlayerBackend.TransferObjects
open MusicPlayerBackend.TransferObjects.Track

[<ApiController>]
[<Consumes(MediaTypeNames.Application.Json)>]
[<Produces(MediaTypeNames.Application.Json)>]
[<ProducesResponseType(StatusCodes.Status400BadRequest)>]
[<ProducesResponseType(typeof<UnauthorizedResponse>, StatusCodes.Status401Unauthorized)>]
[<Authorize>]
[<Route("[controller]")>]
type TrackController(trackRepository: FsharpTrackRepository,
                     s3Service: S3Service,
                     unitOfWork: FsharpUnitOfWork,
                     userProvider: UserProvider) =
    inherit ControllerBase()

    /// <summary>
    ///     Gets track by Id.
    /// </summary>
    [<HttpGet("{id:guid}", Name = "GetTrack")>]
    [<ProducesResponseType(typeof<TrackResponse>, StatusCodes.Status200OK)>]
    member this.Get(id: Guid, ct: CancellationToken) = task {
        let! userId = userProvider.GetUserId()
        let! track = trackRepository.TryGetVisible(id, userId, ct)
        match track with
        | None ->
            return this.NotFound() :> IActionResult
        | Some track ->
            let track = TrackResponse(
                Id = track.Id,
                CoverUri = (track.CoverUri |> Option.toObj),
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
        let! track = trackRepository.TryGetIfOwner(userId, id, ct)
        match track with
        | None ->
            return this.NotFound() :> IActionResult
        | Some track ->
            if request.Visibility.HasValue then
                track.Visibility <-
                    match request.Visibility.Value with
                    | TrackVisibility.Hidden -> Entities.TrackVisibility.Hidden
                    | TrackVisibility.Visible -> Entities.TrackVisibility.Visible
                    | _ -> ArgumentOutOfRangeException() |> raise

            if request.Cover <> null then
                if request.RemoveCover then
                    return this.BadRequest(UpdateTrackErrorResponse(Error = $"{nameof(request.RemoveCover)} is true")) :> IActionResult
                else
                    let! uploadedCoverUri =
                        s3Service
                            .TryUploadFileStream(
                                "covers",
                                Guid.NewGuid().ToString() + "_" + track.Name,
                                request.Cover.OpenReadStream(),
                                Path.GetExtension(request.Cover.FileName)
                            )

                    if Option.isNone uploadedCoverUri then
                        return this.BadRequest("Failed to upload cover") :> IActionResult
                    else
                        track.CoverUri <- uploadedCoverUri
                        if request.RemoveCover then
                            track.CoverUri <- None
                            %trackRepository.Save(track)
                            do! unitOfWork.SaveChanges(ct)

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
        let! track = trackRepository.DeleteIfOwner(id, userId, ct)
        do! unitOfWork.SaveChanges(ct)
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
            Path.GetExtension(request.Track.FileName))

        if Option.isNone uploadedTrackUri then
            return this.BadRequest("Failed to upload track") :> IActionResult
        else
            let mutable coverUri = null
            if request.Cover <> null then
                let! uploadedCoverUri = s3Service.TryUploadFileStream(
                    "covers", Guid.NewGuid().ToString() + "_" + request.Name,
                    request.Cover.OpenReadStream(),
                    Path.GetExtension(request.Cover.FileName))

                if Option.isNone uploadedCoverUri then
                    return this.BadRequest("Failed to upload cover") :> IActionResult
                else
                    coverUri <- uploadedCoverUri.Value

                    let! ownerUserId = userProvider.GetUserId()
                    let xv =
                        match request.Visibility with
                        | TrackVisibility.Hidden -> Entities.TrackVisibility.Hidden
                        | TrackVisibility.Visible -> Entities.TrackVisibility.Visible
                        | _ -> ArgumentOutOfRangeException() |> raise

                    let track = Track.Create(ownerUserId, (coverUri |> Option.ofObj), uploadedTrackUri.Value, xv, request.Name, request.Author)

                    %trackRepository.Save(track)
                    do! unitOfWork.SaveChanges(ct)
                    let track = TrackResponse(
                        Id = track.Id,
                        CoverUri = (track.CoverUri |> Option.toObj),
                        TrackUri = track.TrackUri,
                        Visibility = ucast track.Visibility,
                        Name = track.Name,
                        Author = track.Author
                    )
                    return this.Ok(track) :> IActionResult
            else
                return this.NoContent() :> IActionResult
    }


