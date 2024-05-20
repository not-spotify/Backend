module Domain.Tests.Playlist.CreatePlaylistTests

open Domain.Playlist
open Domain.Shared
open NUnit.Framework

open System

open MusicPlayerBackend.Common
open Contracts.Shared
open Domain.User

let testStore (playlists: Playlist seq) : PlaylistStore =
    let playlists = ResizeArray(playlists)

    let save playlist = task {
        return Ok playlist
    }

    let isPlaylistExist (playlistName, userId) = task {
        return playlists |> Seq.exists ^ fun p -> p.Name = playlistName && p.UserId = userId
    }

    let tryGetByUserId userId = task {
        return playlists
               |> Seq.filter ^ fun p -> p.UserId = userId
               |> Array.ofSeq
    }

    let tryGetById playlistId = task {
        return playlists |> Seq.tryFind ^ fun p -> p.Id = playlistId
    }

    let tryGetByName (playlistName, userId) = task {
        return playlists |> Seq.tryFind ^ fun p -> p.Name = playlistName && p.UserId = userId
    }

    { GetByUserId = tryGetByUserId
      TryGetById = tryGetById
      TryGetByName = tryGetByName
      IsPlaylistExist = isPlaylistExist
      Save = save }

[<Test>]
let ``Try create playlist with duplicated name`` () = task {
    let existingUserId = UserId "24fbcccf-cc5a-4343-b2ca-b329f97c473a"
    let testStore = testStore [|
                                 { CreatedAt = DateTimeOffset.UtcNow
                                   UpdatedAt = None
                                   Id = Id.create()
                                   Name = PlaylistName "duplicated name"
                                   UserId = existingUserId }
                              |]

    let busMock : Bus = {
        Publish = fun _ -> Task.completed
    }

    let request : CreatePlaylistRequest = {
        UserId = existingUserId
        Name = "duplicated name"
    }

    let! createUserResponse = createPlaylist testStore busMock request

    match createUserResponse with
    | Error (CreatePlaylistError.Failed [ CreatePlaylistFailed.DuplicatePlaylistName ]) ->
        Assert.Pass()

    | other ->
        Assert.Fail(string other)
}
