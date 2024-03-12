namespace MusicPlayerBackend.Host.Services

open System
open System.Linq
open Microsoft.EntityFrameworkCore

open MusicPlayerBackend.Contracts
open MusicPlayerBackend.Persistence
open MusicPlayerBackend.Common.TypeExtensions
open MusicPlayerBackend.Persistence.Entities

module PlaylistService =
    let create
        (unitOfWork: FsharpUnitOfWork)
        (playlistRepository: FsharpPlaylistRepository)
        (request: Playlist.CreateRequest) = task {

        if
            request.Name
            |> Option.ofStringW
            |> Option.isNone then
            return Error Playlist.CreatePlaylistError.Empty
        else
            let playlist : Entities.Playlist =
                { Id = Guid.Empty
                  Name = request.Name
                  Visibility =
                      match request.Visibility with
                      | Playlist.Private ->
                          Visibility.Private
                      | Playlist.Public ->
                          Visibility.Public
                  CoverUri = request.CoverFileLink
                  OwnerUserId = request.UserId
                  CreatedAt = DateTimeOffset.MinValue
                  UpdatedAt = None
                  OwnerUser = Unchecked.defaultof<_>
                  TrackPlaylists = null
                  Permissions = null }

            let tracked = playlistRepository.Save(playlist)
            do! unitOfWork.SaveChanges()

            let playlist = tracked.Entity
            let playlist : Playlist.Playlist =
                { Id = playlist.Id
                  Name = playlist.Name
                  CoverUri = playlist.CoverUri
                  OwnerUserId = playlist.OwnerUserId
                  Visibility =
                      match playlist.Visibility with
                      | Visibility.Private -> Playlist.Private
                      | Visibility.Public -> Playlist.Public
                      | _ -> ArgumentOutOfRangeException() |> raise}

            return Ok(playlist)
    }

    let list
        (playlistRepository: FsharpPlaylistRepository)
        (msg: Playlist.ListQuery) = task {
        let getPlaylistListQuery =
            query {
                for playlist in playlistRepository.Query do
                select ({
                    Id = playlist.Id
                    Name = playlist.Name
                    OwnerUserId = playlist.OwnerUserId
                    CoverUri = playlist.CoverUri
                    Visibility =
                        match playlist.Visibility with
                        | Visibility.Private -> Playlist.Visibility.Private
                        | Visibility.Public -> Playlist.Visibility.Public
                        | _ -> ArgumentOutOfRangeException() |> raise
                } : Playlist.Playlist)
            }

        let! totalCount = getPlaylistListQuery.CountAsync()
        let! items =
            getPlaylistListQuery
                .Skip(msg.PageNumber * msg.PageSize)
                .Take(msg.PageSize)
                .ToArrayAsync()

        let list : Playlist.List = {
            Items = items
            PageNumber = msg.PageNumber
            TotalCount = totalCount
        }
        return Ok(list)
    }

    let update
        (unitOfWork: FsharpUnitOfWork)
        (playlistRepository: FsharpPlaylistRepository)
        (msg: Playlist.UpdateRequest) = task {

        let! playlist = playlistRepository.TryGetById(msg.Id)

        match playlist with
        | None ->
            return Error Playlist.UpdatePlaylistError.NonExistingPlaylist
        | Some playlist ->
            // TODO: think about data and message validation.
            if msg.Name |> Option.ofStringW |> Option.isSome then
                playlist.Name <- msg.Name

            if msg.CoverFileLink |> Option.isSome then
                playlist.CoverUri <- msg.CoverFileLink

            let tracked = playlistRepository.Save(playlist)
            do! unitOfWork.SaveChanges()

            let playlist = tracked.Entity
            let playlist : Playlist.Playlist =
                { Id = playlist.Id
                  Name = playlist.Name
                  CoverUri = playlist.CoverUri
                  OwnerUserId = playlist.OwnerUserId
                  Visibility =
                      match playlist.Visibility with
                      | Visibility.Private -> Playlist.Private
                      | Visibility.Public -> Playlist.Public
                      | _ -> ArgumentOutOfRangeException() |> raise}

            return Ok(playlist)
    }
