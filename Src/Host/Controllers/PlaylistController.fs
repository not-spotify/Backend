namespace MusicPlayerBackend.Controllers

open System
open System.IO
open System.Net.Mime
open System.Threading
open System.Threading.Tasks
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc

open MusicPlayerBackend.Common
open MusicPlayerBackend.Contracts.Playlist
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
type PlaylistController(
    playlistRepository: FsharpPlaylistRepository,
    unitOfWork: FsharpUnitOfWork,
    s3Service: S3Service,
    userProvider: UserProvider) =
    inherit ControllerBase()

    /// <summary>
    ///     Gets playlist information.
    /// </summary>
    [<HttpGet("{id:guid}", Name = "GetPlaylist")>]
    [<ProducesResponseType(StatusCodes.Status400BadRequest)>]
    [<ProducesResponseType(typeof<Playlist>, StatusCodes.Status200OK)>]
    member this.Get(id: Guid) = task {
        let! userId = userProvider.GetUserId()
        let! playlist = playlistRepository.TryGetIfCanViewById(id, userId)
        match playlist with
        | None ->
            return this.NotFound({ Error = $"Can't find track {id}" }) :> IActionResult
        | Some playlist ->
            let visibility =
                match playlist.Visibility with
                | Visibility.Private -> PlaylistVisibility.Private
                | Visibility.Public -> PlaylistVisibility.Public
                | _ -> ArgumentOutOfRangeException() |> raise

            return this.Ok({ Id = playlist.Id
                             Name = playlist.Name
                             CoverUri = playlist.CoverUri
                             Visibility = visibility } : Models.Playlist) :> IActionResult
    }

    [<HttpGet("{id:guid}", Name = "SearchPlaylistTracks")>]
    [<ProducesResponseType(StatusCodes.Status400BadRequest)>]
    [<ProducesResponseType(typeof<Playlist>, StatusCodes.Status200OK)>]
    member this.SearchPlaylistTracks() = task {
        ()
    }

    /// <summary>
    ///     Creates new playlist.
    /// </summary>
    [<HttpPost(Name = "CreatePlaylist")>]
    [<Authorize>]
    [<ProducesResponseType(typeof<Playlist>, StatusCodes.Status200OK)>]
    [<ProducesResponseType(StatusCodes.Status400BadRequest)>]
    member this.Create([<FromBody>] request: CreatePlaylist) = task {
        let! userId = userProvider.GetUserId()

        let! coverUri =
            match request.Cover with
            | None -> Task.FromResult None
            | Some cover ->
                s3Service.TryUploadFileStream(
                    "covers", Guid.NewGuid().ToString() + "_" + request.Name,
                    cover.OpenReadStream(),
                    Path.GetExtension(cover.FileName))

        let visibility : MusicPlayerBackend.Contracts.Playlist.Visibility =
            match request.Visibility with
            | PlaylistVisibility.Private -> MusicPlayerBackend.Contracts.Playlist.Visibility.Private
            | PlaylistVisibility.Public -> MusicPlayerBackend.Contracts.Playlist.Visibility.Public

        let msg : CreateRequest = {
            UserId = userId
            Name = request.Name
            Visibility = visibility
            CoverFileLink = coverUri
        }

        let! playlist = msg |> PlaylistService.create unitOfWork playlistRepository
        match playlist with
        | Error error ->
            // TODO: Remove cover
            return this.BadRequest({
                Error = string error
            } : BadResponse) :> IActionResult

        | Ok playlist ->
            return this.Ok({ Id = playlist.Id
                             Name = playlist.Name
                             CoverUri = playlist.CoverUri
                             Visibility = request.Visibility } : Models.Playlist) :> IActionResult
    }

    /// <summary>
    ///     Deletes playlist.
    /// </summary>
    [<HttpDelete("{id:guid}", Name = "DeletePlaylist")>]
    [<ProducesResponseType(StatusCodes.Status204NoContent)>]
    [<ProducesResponseType(StatusCodes.Status400BadRequest)>]
    member this.Delete(id: Guid, ct: CancellationToken) = task {
        let! userId = userProvider.GetUserId()
        let! playlist = playlistRepository.TryGetIfCanModifyById(id, userId, ct)
        match playlist with
        | None ->
            return this.BadRequest({
                Error = $"Can't find playlist with id {id}"
            } : BadResponse) :> IActionResult
        | Some playlist ->
            playlistRepository.Delete(playlist)
            do! unitOfWork.Commit()
            return this.NoContent() :> IActionResult
    }

    /// <summary>
    ///     Updates name or visibility level of playlist.
    /// </summary>
    [<HttpPut("{playlistId:guid}", Name = "UpdatePlaylist")>]
    [<ProducesResponseType(StatusCodes.Status204NoContent)>]
    [<Consumes(MediaTypeNames.Application.FormUrlEncoded)>]
    member this.Update(playlistId: Guid, [<FromForm>] request: UpdatePlaylist) = task {
        let! cover =
            request.Cover
            |> Option.map ^ fun cover -> s3Service.TryUploadFileStream(
                "bucket",
                Guid.NewGuid().ToString() + "_" + cover.FileName,
                cover.OpenReadStream(),
                ".jpg")
            |> Option.defaultValue ^ Task.fromResult None

        let! userId = userProvider.GetUserId()
        let msg = {
            UserId = userId
            Id = playlistId
            Name = request.Name
            DeleteCover = request.DeleteCover
            CoverFileLink = cover
        }
        let! playlist = msg |> PlaylistService.update unitOfWork playlistRepository

        if Result.isError playlist then
            () // TODO: Remove cover

        return this.Ok(playlist) :> IActionResult
    }

    /// <summary>
    ///     Get visible to user playlists.
    /// </summary>
    [<HttpGet("{playlistId:guid}/Tracks", Name = "GetPlaylists")>]
    [<ProducesResponseType(typeof<ItemsResponse<Playlist>>, StatusCodes.Status200OK)>]
    member this.List(request: SearchTracksRequest) = task {
        let! userId = userProvider.GetUserId()
        let msg : ListQuery = {
            UserId = userId
            PageNumber = request.Page
            PageSize = request.PageSize
        }
        let! listResponse = msg |> PlaylistService.list playlistRepository
        match listResponse with
        | Error error ->
            return this.BadRequest({
                Error = string error
            } : BadResponse) :> IActionResult
        | Ok list ->
            let tracks =
                list.Items
                |> Array.map ^ fun playlist -> {
                    Id = playlist.Id
                    Name = playlist.Name
                    CoverUri = playlist.CoverUri
                    Visibility =
                        match playlist.Visibility with
                        | MusicPlayerBackend.Contracts.Playlist.Private ->
                            PlaylistVisibility.Private
                        | MusicPlayerBackend.Contracts.Playlist.Public ->
                            PlaylistVisibility.Public
                }
            return this.Ok({ PageNumber = list.PageNumber
                             TotalCount = list.TotalCount
                             Items = tracks } : ItemsResponse<Models.Playlist>) :> IActionResult
    }
