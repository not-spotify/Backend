namespace MusicPlayerBackend.Controllers

open System
open System.IO
open System.Net.Mime
open System.Threading.Tasks
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc

open MusicPlayerBackend.Common
open MusicPlayerBackend.Host
open MusicPlayerBackend.Host.Contracts.Playlist
open MusicPlayerBackend.Host.Models.Common
open MusicPlayerBackend.Host.Models.Playlist
open MusicPlayerBackend.Host.Models.Track
open MusicPlayerBackend.Host.Services
open MusicPlayerBackend.Persistence
open MusicPlayerBackend.TransferObjects
open MusicPlayerBackend.TransferObjects.Playlist

type ReturnBadRequestException(ar: IActionResult) =
    inherit Exception()

    member _.ActionResult with get() = ar

[<ApiController>]
[<Consumes(MediaTypeNames.Application.Json)>]
[<Produces(MediaTypeNames.Application.Json)>]
[<ProducesResponseType(StatusCodes.Status400BadRequest)>]
[<ProducesResponseType(typeof<UnauthorizedResponse>, StatusCodes.Status401Unauthorized)>]
[<Authorize>]
[<Route("[controller]")>]
type PlaylistController(
    playlistRepository: FsharpPlaylistRepository,
    trackRepository: FsharpTrackRepository,
    trackPlaylistRepository: FsharpTrackPlaylistRepository,
    playlistUserPermissionRepository: FsharpPlaylistUserPermissionRepository,
    unitOfWork: FsharpUnitOfWork,
    s3Service: S3Service,
    userProvider: UserProvider) =
    inherit ControllerBase()

    // /// <summary>
    // ///     Get visible to user playlists if authorized.
    // ///     For unauthorized users only visible playlists will be returned.
    // /// </summary>
    // [<AllowAnonymous>]
    // [<HttpGet("List", Name = "GetPlaylists")>]
    // [<ProducesResponseType(StatusCodes.Status400BadRequest)>]
    // [<ProducesResponseType(typeof<PlaylistListResponse>, StatusCodes.Status200OK)>]
    // member this.List([<FromQuery>] request: PlaylistListRequest, ct: CancellationToken) = task {
    //         let! userId = userProvider.TryGetUserId()
    //         let visiblePlaylistsQuery =
    //             match userId with
    //             | Some userId ->
    //                 query {
    //                     for playlist in playlistRepository.QueryAll() do
    //                     where (playlist.Visibility = PlaylistVisibility.Public || playlist.OwnerUserId = userId || playlist.Permissions.Any(fun np -> np.UserId = userId && np.PlaylistId = playlist.Id))
    //                     select playlist
    //                 }
    //             | None ->
    //                 query {
    //                     for playlist in playlistRepository.QueryAll() do
    //                     where (playlist.Visibility = PlaylistVisibility.Public)
    //                 }
    //         let! totalCount = visiblePlaylistsQuery.CountAsync(ct)
    //         let playlists =
    //             query {
    //                 for playlist in visiblePlaylistsQuery do
    //                 skip (request.PageSize * request.Page)
    //                 take request.PageSize
    //                 select (PlaylistListItemResponse(
    //                     Id = playlist.Id,
    //                     CoverUri = playlist.CoverUri,
    //                     Name = playlist.Name,
    //                     Visibility = ucast playlist.Visibility))
    //             }
    //         let! playlists = playlists.ToArrayAsync(ct)
    //
    //         return this.Ok(PlaylistListResponse(Items = playlists, TotalCount = totalCount))
    //     }
    //
    // /// <summary>
    // ///     Gets playlist information.
    // /// </summary>
    // [<HttpGet("{id:guid}", Name = "GetPlaylist")>]
    // [<ProducesResponseType(StatusCodes.Status400BadRequest)>]
    // [<ProducesResponseType(typeof<PlaylistResponse>, StatusCodes.Status200OK)>]
    // member this.Get(id: Guid, ct: CancellationToken) = task {
    //     let! userId = userProvider.GetUserId()
    //     let visiblePlaylistsQuery =
    //         query {
    //             for p in playlistRepository.QueryAll() do
    //             where (
    //                 p.Visibility = PlaylistVisibility.Public
    //                 || p.OwnerUserId = userId
    //                 || p.Permissions.Any(fun np -> np.UserId = userId && np.PlaylistId = p.Id)
    //             )
    //         }
    //
    //     let! playlist = visiblePlaylistsQuery.SingleOrDefaultAsync((fun p -> p.Id = id), cancellationToken = ct)
    //     if playlist = null then
    //         return this.NotFound() :> IActionResult
    //     else
    //         let playlist = PlaylistResponse(
    //             Id = playlist.Id,
    //             CoverUri = playlist.CoverUri,
    //             Name = playlist.Name
    //         )
    //         return this.Ok(playlist) :> IActionResult
    // }

    /// <summary>
    ///     Creates new playlist.
    /// </summary>
    [<HttpPost(Name = "CreatePlaylist")>]
    [<Authorize>]
    [<ProducesResponseType(typeof<PlaylistResponse>, StatusCodes.Status200OK)>]
    [<ProducesResponseType(StatusCodes.Status400BadRequest)>]
    member this.Create([<FromBody>] request: PlaylistCreateRequest) = task {
        let! userId = userProvider.GetUserId()

        let! coverUri =
            match request.Cover with
            | None -> Task.FromResult None
            | Some cover ->
                s3Service.TryUploadFileStream(
                    "covers", Guid.NewGuid().ToString() + "_" + request.Name,
                    cover.OpenReadStream(),
                    Path.GetExtension(cover.FileName))

        let visibility =
            match request.Visibility with
            | PlaylistVisibility.Private ->
                Visibility.Private
            | PlaylistVisibility.Public ->
                Visibility.Public

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
                             Visibility = request.Visibility } : PlaylistResponse) :> IActionResult
    }

    // /// <summary>
    // ///     Creates clone of existing playlist.
    // /// </summary>
    // [<HttpPost("{id:guid}/Clone", Name = "ClonePlaylist")>]
    // [<ProducesResponseType(typeof<PlaylistResponse>, StatusCodes.Status200OK)>]
    // [<ProducesResponseType(StatusCodes.Status400BadRequest)>]
    // member this.Clone(id: Guid, request: ClonePlaylistRequest, ct: CancellationToken) = task {
    //     try
    //         let! user = userProvider.GetUser()
    //
    //         if user.FavoritePlaylistId = id then
    //             raise ^ ReturnBadRequestException(this.BadRequest(UpdatePlaylistErrorResponse("You can't create copy of liked playlist")) :> IActionResult)
    //
    //         let! playlist = playlistRepository.GetByIdOrDefaultAsync(id, ct)
    //         if playlist = null then
    //             raise ^ ReturnBadRequestException(this.BadRequest(UpdatePlaylistErrorResponse($"Can't find playlist with id {id}")) :> IActionResult)
    //
    //         let! currentUserId = userProvider.GetUserId()
    //         let! createdPlaylistNames = playlistRepository.GetManyAsync((fun p -> p.OwnerUserId = currentUserId), (fun p -> p.Name), ct)
    //
    //         if createdPlaylistNames.Length >= 10 then
    //             raise ^ ReturnBadRequestException(this.BadRequest(UpdatePlaylistErrorResponse("Current limit of playlists is 10. Sorry.")) :> IActionResult)
    //
    //         let originalPlaylistId = playlist.Id
    //         let! x = playlistUserPermissionRepository.HasAccessForView(originalPlaylistId, currentUserId, ct)
    //         if playlist.OwnerUserId <> currentUserId && not x then
    //             raise ^ ReturnBadRequestException(this.BadRequest(UpdatePlaylistErrorResponse($"Can't find playlist with id {originalPlaylistId}")) :> IActionResult)
    //
    //         let! _ =  unitOfWork.BeginTransaction(ct)
    //         let mutable playlistName = playlist.Name
    //         if not (String.IsNullOrWhiteSpace(request.Name.Trim())) then
    //             playlistName <- request.Name
    //             if createdPlaylistNames.Contains(playlistName) then
    //                 playlistName <- playlistName + " (1)"
    //
    //         playlist.Id <- Guid.Empty
    //         playlist.CreatedAt <- DateTimeOffset.UtcNow
    //         playlist.UpdatedAt <- System.Nullable()
    //         playlist.Name <- playlistName
    //         playlistRepository.Save(playlist)
    //         do! unitOfWork.BeginTransaction(ct)
    //
    //         let! tracks =
    //             query {
    //                 for tp in trackPlaylistRepository.QueryAll() do
    //                 where (tp.PlaylistId = originalPlaylistId)
    //                 where (tp.Track.Visibility = TrackVisibility.Visible || tp.Track.OwnerUserId = user.Id)
    //             } |> _.AsNoTracking().ToArrayAsync(ct)
    //
    //         for trackPlaylist in tracks do
    //             trackPlaylist.Id <- Guid.Empty
    //             trackPlaylist.PlaylistId <- playlist.Id
    //             trackPlaylistRepository.Save(trackPlaylist)
    //
    //         do! unitOfWork.BeginTransaction(ct)
    //         do! unitOfWork.Commit(ct)
    //
    //         return this.Ok(playlist) :> IActionResult
    //     with :? ReturnBadRequestException as br -> // TODO: hack. rewrite.
    //         return this.BadRequest(br.ActionResult)
    // }
    //
    // /// <summary>
    // ///     Deletes playlist.
    // /// </summary>
    // [<HttpDelete("{id:guid}", Name = "DeletePlaylist")>]
    // [<ProducesResponseType(StatusCodes.Status204NoContent)>]
    // [<ProducesResponseType(StatusCodes.Status400BadRequest)>]
    // member this.Delete(id: Guid) = task {
    //     let! playlist = playlistRepository.GetByIdOrDefaultAsync(id)
    //     if playlist = null then
    //         return this.BadRequest(UpdatePlaylistErrorResponse($"Can't find playlist with id {id}")) :> IActionResult
    //     else
    //         playlistRepository.Delete(playlist)
    //         do! unitOfWork.BeginTransaction()
    //         return this.NoContent() :> IActionResult
    // }
    //
    // /// <summary>
    // ///     Updates bunch of tracks.
    // /// </summary>
    // [<HttpPut("{playlistId:guid}/Tracks", Name = "EditTracks")>]
    // [<ProducesResponseType(typeof<BulkTrackActionResponse>, StatusCodes.Status200OK)>]
    // [<ProducesResponseType(StatusCodes.Status400BadRequest)>]
    // member this.BulkTrackActions(playlistId: Guid, request: BulkTrackActionRequest, ct: CancellationToken) = task {
    //     let! playlist = playlistRepository.GetByIdOrDefaultAsync(playlistId, ct)
    //     try
    //         if playlist = null then
    //             raise ^ ReturnBadRequestException(this.BadRequest(UpdatePlaylistErrorResponse($"Can't find playlist with id {playlistId}")) :> IActionResult)
    //
    //         let! userId = userProvider.GetUserId()
    //         let! hasAccessForChange = playlistUserPermissionRepository.HasAccessForChange(playlist.Id, userId, ct)
    //         if playlist.OwnerUserId <> userId && not hasAccessForChange then
    //             raise ^ ReturnBadRequestException(this.BadRequest(UpdatePlaylistErrorResponse($"Can't find playlist with id {playlistId}")) :> IActionResult)
    //
    //         let trackIds = request.Tracks |> Seq.map (_.Id) |> Seq.distinct
    //         let! tracks = trackRepository.GetByIdsAsync(trackIds, ct)
    //         let tracks = tracks |> Seq.map (fun track -> track.Id, track) |> dict
    //         let! addedTracks = trackPlaylistRepository.QueryMany((fun tp -> tp.PlaylistId = playlistId), fun tp -> tp.TrackId).ToListAsync(ct)
    //
    //         let mutable responseTrackItems = List<TrackResponseItem>(request.Tracks.Length)
    //         for track in request.Tracks do
    //             let id = track.Id
    //             if not (tracks.ContainsKey(id)) then
    //                 responseTrackItems.Add(TrackResponseItem(Id = id, Action = TrackActionResponse.NotFound))
    //             else
    //                 match track.Action with
    //                 | TrackActionRequest.Delete ->
    //                     let! trackPlaylist =
    //                         query {
    //                             for tp in trackPlaylistRepository.QueryAll() do
    //                                 where (tp.PlaylistId = playlistId && tp.TrackId = id)
    //                         } |> _.SingleAsync(ct)
    //
    //                     trackPlaylistRepository.Delete(trackPlaylist)
    //                     %addedTracks.Remove(id)
    //                     responseTrackItems.Add(TrackResponseItem(Id = id, Action = TrackActionResponse.Deleted))
    //                 | TrackActionRequest.Add ->
    //                     if not (addedTracks.Contains(id)) then
    //                         trackPlaylistRepository.Save(TrackPlaylist(PlaylistId = playlistId, TrackId = id))
    //                         responseTrackItems.Add(TrackResponseItem(Id = id, Action = TrackActionResponse.Added))
    //                     else
    //                         responseTrackItems.Add(TrackResponseItem(Id = id, Action = TrackActionResponse.AlreadyAdded))
    //                 | _ -> ArgumentOutOfRangeException() |> raise
    //         do! unitOfWork.BeginTransaction(ct)
    //         return this.Ok(BulkTrackActionResponse(Tracks = responseTrackItems.ToArray())) :> IActionResult
    //     with :? ReturnBadRequestException as br -> // TODO: hack. rewrite.
    //         return this.BadRequest(br.ActionResult)
    // }

    /// <summary>
    ///     Updates name or visibility level of playlist.
    /// </summary>
    [<HttpPut("{playlistId:guid}", Name = "EditPlaylist")>]
    [<ProducesResponseType(StatusCodes.Status204NoContent)>]
    [<ProducesResponseType(typeof<UpdatePlaylistErrorResponse>, StatusCodes.Status400BadRequest)>]
    [<Consumes(MediaTypeNames.Application.FormUrlEncoded)>]
    member this.Update(playlistId: Guid, [<FromForm>] msg: UpdateRequest) = task {
        let! playlist = msg |> PlaylistService.update unitOfWork playlistRepository

        if Result.isError playlist then
            () // TODO: Remove cover

        return this.Ok(playlist) :> IActionResult
    }

    /// <summary>
    ///     Gets track list
    /// </summary>
    [<HttpGet("{playlistId:guid}/Tracks", Name = "GetPlaylists")>]
    [<ProducesResponseType(typeof<ItemsResponse<PlaylistResponse>>, StatusCodes.Status200OK)>]
    member this.List(request: TrackFilterRequest) = task {
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
            let tracks = list.Items |> Array.map ^ fun p -> { Id = p.Id
                                                              Name = p.Name
                                                              CoverUri = p.CoverUri
                                                              Visibility =
                                                                  match p.Visibility with
                                                                  | Visibility.Private ->
                                                                      PlaylistVisibility.Private
                                                                  | Visibility.Public ->
                                                                      PlaylistVisibility.Public } : PlaylistResponse
            return this.Ok({ PageNumber = list.PageNumber
                             TotalCount = list.TotalCount
                             Items = tracks } : ItemsResponse<PlaylistResponse>) :> IActionResult
    }
