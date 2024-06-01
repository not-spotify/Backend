module Domain.Playlist

open System
open System.Runtime.CompilerServices
open System.Threading.Tasks
open Contracts.Shared
open Domain.Shared
open Domain.User

open FsToolkit.ErrorHandling

[<assembly: InternalsVisibleTo("Domain.Tests")>]
do()

type PlaylistId = Id
type TrackId = Id

type Playlist = {
    Id: PlaylistId
    Name: PlaylistName
    UserId: UserId
    CreatedAt: DateTimeOffset
    UpdatedAt: DateTimeOffset option
}

type PlaylistFieldValidationFailed =
    | Name of PlaylistNameError
    | UnknownError of string

type CreatePlaylistFailed =
    | DuplicatePlaylistName

type CreatePlaylistError =
    | Failed of CreatePlaylistFailed list
    | ValidationFailed of PlaylistFieldValidationFailed list

type RemoveTrackFromPlaylistError =
    | PlaylistNotFound

type AddTrackToPlaylistError =
    | PlaylistNotFound

[<NoComparison; NoEquality>]
type PlaylistService = {
    UserHasAccessToPlaylist: UserId * PlaylistId -> Task<bool>
}

[<NoComparison; NoEquality>]
type PlaylistStore = {
    GetByUserId: UserId -> Task<Playlist[]>
    TryGetById: PlaylistId -> Task<Playlist option>
    TryGetByName: PlaylistName * UserId -> Task<Playlist option>
    IsPlaylistExist: PlaylistName * UserId -> Task<bool>
    Remove: PlaylistId -> Task
    Save: Playlist -> TaskResult<Playlist, CreatePlaylistError>
}

type CreatePlaylistRequest = {
    UserId: UserId
    Name: string
}

type CreatePlaylist = CreatePlaylistRequest -> TaskResult<Playlist, CreatePlaylistError>

let createPlaylistInternal userId (request: CreatePlaylistRequest) =
    validation {
        let! name = request.Name
                    |> PlaylistName.create
                    |> Result.mapError PlaylistFieldValidationFailed.Name

        return {
            Id = Id.create()
            Name = name
            UserId = userId
            CreatedAt = DateTimeOffset.UtcNow
            UpdatedAt = None }
    }


let createPlaylist (playlistStore: PlaylistStore) (bus: Bus) : CreatePlaylist =
    fun request -> taskResult {
        let! newPlaylist = createPlaylistInternal request.UserId request
                           |> Result.mapError CreatePlaylistError.ValidationFailed

        do! playlistStore.IsPlaylistExist(newPlaylist.Name, request.UserId)
            |> TaskResult.requireFalse (CreatePlaylistError.Failed [ CreatePlaylistFailed.DuplicatePlaylistName ])

        let! playlist = playlistStore.Save(newPlaylist)
        do! bus.Publish(playlist)

        return playlist
    }


type RemovePlaylistRequest = {
    UserId: UserId
    Id: PlaylistId
}

type RemovePlaylistError =
    | PlaylistNotFound

type RemovePlaylist = RemovePlaylistRequest -> TaskResult<Unit, RemovePlaylistError>


let removePlaylist (playlistStore: PlaylistStore) (bus: Bus) : RemovePlaylist =
    fun request -> taskResult {
        let! playlist = playlistStore.TryGetById(request.Id) |> TaskResult.requireSome PlaylistNotFound
        do! playlist.UserId = request.UserId |> Result.requireTrue PlaylistNotFound

        let! playlist = playlistStore.Remove(request.Id)
        do! bus.Publish(playlist)

        return playlist
    }


type AddTrackToPlaylistRequest = {
    UserId: UserId
    TrackId: TrackId
    PlaylistId: PlaylistId
}

type AddedTrackToPlaylistEvent = {
    UserId: UserId
    TrackId: TrackId
    PlaylistId: PlaylistId
}

type RemoveTrackToPlaylistRequest = {
    UserId: UserId
    TrackId: TrackId
    PlaylistId: PlaylistId
}

type RemovedTrackFromPlaylistEvent = {
    UserId: UserId
    TrackId: TrackId
    PlaylistId: PlaylistId
}

type AddTrackToPlaylist = AddTrackToPlaylistRequest -> TaskResult<AddedTrackToPlaylistEvent, AddTrackToPlaylistError>
type RemoveTrackFromPlaylist = AddTrackToPlaylistRequest -> TaskResult<RemovedTrackFromPlaylistEvent, RemoveTrackFromPlaylistError>

[<NoComparison; NoEquality>]
type TrackPlaylistStore = {
    Add: TrackId * PlaylistId -> Task
    Remove: TrackId * PlaylistId -> Task
}

let addTrackToPlaylist (playlistService: PlaylistService) (trackPlaylistStore: TrackPlaylistStore) (bus: Bus) : AddTrackToPlaylist =
    fun request -> taskResult {
        do! playlistService.UserHasAccessToPlaylist(request.UserId, request.PlaylistId)
            |> TaskResult.requireTrue AddTrackToPlaylistError.PlaylistNotFound

        do! trackPlaylistStore.Add(request.TrackId, request.PlaylistId)

        let event : AddedTrackToPlaylistEvent =
            { UserId = request.UserId
              TrackId = request.TrackId
              PlaylistId = request.PlaylistId }

        do! bus.Publish(event)

        return event
    }

let removeTrackFromPlaylist (playlistService: PlaylistService) (trackPlaylistStore: TrackPlaylistStore) (bus: Bus) : RemoveTrackFromPlaylist =
    fun request -> taskResult {
        do! playlistService.UserHasAccessToPlaylist(request.UserId, request.PlaylistId)
            |> TaskResult.requireTrue RemoveTrackFromPlaylistError.PlaylistNotFound

        do! trackPlaylistStore.Remove(request.TrackId, request.PlaylistId)

        let event : RemovedTrackFromPlaylistEvent =
            { UserId = request.UserId
              TrackId = request.TrackId
              PlaylistId = request.PlaylistId }

        do! bus.Publish(event)

        return event
    }
