module Domain.Track

open System
open System.IO
open System.Runtime.CompilerServices
open System.Threading.Tasks
open Contracts.Shared
open Domain.User
open FsToolkit.ErrorHandling
open Domain.Shared

[<assembly: InternalsVisibleTo("Domain.Tests")>]
do()

type TrackId = Playlist.TrackId
type PlaylistId = Playlist.PlaylistId

type Track = {
    Id: TrackId
    Name: TrackName
    Author: AuthorName
    FileUri: string
    CreatedAt: DateTimeOffset
    UpdatedAt: DateTimeOffset option
}

type CreateTrackRequest = {
    PlaylistId: PlaylistId
    Name: string
    Author: string
}

type TrackFieldValidationFailed =
    | Name of TrackNameError
    | Author of AuthorNameError
    | UnknownError of string

type CreateTrackFailed =
    | PlaylistNotFound

type CreateTrackError =
    | Failed of CreateTrackFailed list
    | ValidationFailed of TrackFieldValidationFailed list

type UploadTrackFileError = CreateTrackError

type FileUri = string
type UploadTrackFile = Unit -> TaskResult<FileUri, UploadTrackFileError>

type CreateTrack = UserId * CreateTrackRequest -> UploadTrackFile -> TaskResult<Track, CreateTrackError>

let createTrackInternal (request: CreateTrackRequest) =
    validation {
        let! author = request.Author
                      |> AuthorName.create
                      |> Result.mapError TrackFieldValidationFailed.Author

        let! name = request.Name
                    |> TrackName.create
                    |> Result.mapError TrackFieldValidationFailed.Name

        return {
            Id = Id.create()
            Author = author
            Name = name
            CreatedAt = DateTimeOffset.UtcNow
            UpdatedAt = None
            FileUri = null }
    }

[<NoComparison; NoEquality>]
type TrackStore = {
    TryGetByPlaylistId: PlaylistId -> TaskOption<Track[]>
    TryGetById: TrackId -> TaskOption<Track>
    TryGetByNameAuthor: PlaylistId * AuthorName * TrackName -> TaskOption<Track[]>
    Save: Track -> TaskResult<Track, CreateTrackError>
}

[<NoComparison; NoEquality>]
type TrackFileStore = {
    Save: Stream -> Task<FileUri>
    Get: FileUri -> TaskResult<Stream, unit>
}

type PlaylistService = Playlist.PlaylistService

let createTrack (trackStore: TrackStore) (bus: Bus) (playlistService: PlaylistService) : CreateTrack =
    fun (userId, request) uploadFile -> taskResult {
        let! newTrack = createTrackInternal request
                        |> Result.mapError CreateTrackError.ValidationFailed

        do! playlistService.UserHasAccessToPlaylist(userId, request.PlaylistId)
            |> TaskResult.requireTrue (CreateTrackError.Failed [ PlaylistNotFound ])

        let! fileUri = uploadFile ()
        let track = { newTrack with
                        FileUri = fileUri
                        CreatedAt = DateTimeOffset.UtcNow }

        let! track = trackStore.Save(track)
        do! bus.Publish(track)

        return track
    }
