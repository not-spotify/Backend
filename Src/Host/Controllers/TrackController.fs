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
open MusicPlayerBackend.Host.Models
open MusicPlayerBackend.Host.Models.Track
open MusicPlayerBackend.Host.Services
open MusicPlayerBackend.Persistence
open MusicPlayerBackend.Persistence.Entities

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
    member this.Get(id: TrackId, ct: CancellationToken) = task {
        let! userId = userProvider.GetUserId()
        let! track = trackRepository.TryGetVisible(id, userId, ct)
        match track with
        | None ->
            return this.NotFound() :> IActionResult
        | Some track ->
            let visibility =
                match track.Visibility with
                | TrackVisibility.Hidden -> Hidden
                | TrackVisibility.Visible -> Public
                | _ -> ArgumentOutOfRangeException() |> raise

            return this.Ok({
                Id = track.Id
                CoverUri = track.CoverUri
                TrackUri = track.TrackUri
                Visibility = visibility
                Name = track.Name
                Author = track.Author
                CreatedAt = track.CreatedAt
                UpdatedAt = track.UpdatedAt
            } : TrackResponse) :> IActionResult
    }

    /// <summary>
    ///     Updates track's visibility level, cover.
    /// </summary>
    [<HttpPut("{id:guid}", Name = "UpdateTrack")>]
    [<Consumes(MediaTypeNames.Application.FormUrlEncoded)>]
    member this.Update(id: Guid, [<FromForm>] request: UpdateTrackRequest, ct: CancellationToken) = task {
        let! userId = userProvider.GetUserId()
        let! track = trackRepository.TryGetIfOwner(userId, id, ct)
        match track with
        | None ->
            return this.NotFound() :> IActionResult
        | Some track ->
            match request.Visibility with
            | Some visibility ->
                track.Visibility <-
                    match visibility with
                    | Hidden -> TrackVisibility.Hidden
                    | Public -> TrackVisibility.Visible
            | _ -> ()

            match request.Cover with
            | Some cover ->
                if request.RemoveCover then
                    return this.BadRequest({
                        Error = $"{nameof(request.RemoveCover)} is true"
                    } : BadResponse) :> IActionResult
                else
                    let! uploadedCoverUri =
                        s3Service
                            .TryUploadFileStream(
                                "covers",
                                Guid.NewGuid().ToString() + "_" + track.Name,
                                cover.OpenReadStream(),
                                Path.GetExtension(cover.FileName)
                            )

                    if Option.isNone uploadedCoverUri then
                        return this.BadRequest({
                            Error = "Failed to upload cover"
                        } : BadResponse) :> IActionResult
                    else
                        track.CoverUri <- uploadedCoverUri
                        if request.RemoveCover then
                            track.CoverUri <- None
                            %trackRepository.Save(track)
                            do! unitOfWork.SaveChanges(ct)

                        return this.NoContent() :> IActionResult
            | None ->
                return this.NoContent() :> IActionResult
    }

    /// <summary>
    ///     Deletes track.
    /// </summary>
    [<HttpDelete("{id:guid}", Name = "DeleteTrack")>]
    member this.Delete(id: Guid, ct: CancellationToken) = task {
        let! userId = userProvider.GetUserId()
        do! trackRepository.DeleteIfOwner(id, userId, ct)
        do! unitOfWork.SaveChanges(ct)
        return this.NoContent() :> IActionResult
    }

    /// <summary>
    ///     Uploads track.
    /// </summary>
    [<HttpPost(Name = "UploadTrack")>]
    [<Consumes(MediaTypeNames.Multipart.FormData)>]
    [<ProducesResponseType(typeof<TrackResponse>, StatusCodes.Status200OK)>]
    member this.Upload([<FromForm>] request: CreateTrackRequest, ct: CancellationToken) = task {
        let! uploadedTrackUri = s3Service.TryUploadFileStream(
            "tracks",
            Guid.NewGuid().ToString() + "_" + request.Name,
            request.Track.OpenReadStream(),
            Path.GetExtension(request.Track.FileName))

        if Option.isNone uploadedTrackUri then
            return this.BadRequest({
                Error = "Failed to upload track"
            } : BadResponse) :> IActionResult
        else
            let mutable coverUri = null
            match request.Cover with
            | Some cover ->
                let! uploadedCoverUri = s3Service.TryUploadFileStream(
                    "covers", Guid.NewGuid().ToString() + "_" + request.Name,
                    cover.OpenReadStream(),
                    Path.GetExtension(cover.FileName))

                if Option.isNone uploadedCoverUri then
                    return this.BadRequest({
                        Error = "Failed to upload cover"
                    } : BadResponse) :> IActionResult
                else
                    coverUri <- uploadedCoverUri.Value

                    let! ownerUserId = userProvider.GetUserId()
                    let xv =
                        match request.Visibility with
                        | Hidden -> TrackVisibility.Hidden
                        | Public -> TrackVisibility.Visible

                    let track = Track.Create(
                        ownerUserId,
                        (coverUri |> Option.ofObj),
                        uploadedTrackUri.Value,
                        xv,
                        request.Name,
                        request.Author)

                    %trackRepository.Save(track)
                    do! unitOfWork.SaveChanges(ct)
                    return this.Ok({ Id = track.Id
                                     CoverUri = track.CoverUri
                                     TrackUri = track.TrackUri
                                     Author = track.Author
                                     Name = track.Name
                                     Visibility = request.Visibility
                                     CreatedAt = track.CreatedAt
                                     UpdatedAt = track.UpdatedAt } : TrackResponse) :> IActionResult
            | None ->
                return this.NoContent() :> IActionResult
    }


