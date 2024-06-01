module Domain.Tests.Playlist.DeletePlaylistTests

open System.Threading.Tasks
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

    let remove id : Task = task {
        let item = playlists |> Seq.tryFind (fun p -> p.Id = id)
        match item with
        | None -> ()
        | Some item ->
            %playlists.Remove(item)
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
      Save = save
      Remove = remove }

[<Test>]
let ``Try delete playlist`` () = task {
    let existingUserId = UserId "24fbcccf-cc5a-4343-b2ca-b329f97c473a"
    let existingPlaylistId = PlaylistId "24fbcccf-cc5a-4343-ffff-b329f97c473a"
    let testStore = testStore [|
                                 { CreatedAt = DateTimeOffset.UtcNow
                                   UpdatedAt = None
                                   Id = existingPlaylistId
                                   Name = PlaylistName "duplicated name"
                                   UserId = existingUserId }
                              |]

    let busMock : Bus = {
        Publish = fun _ -> Task.completed
    }

    let request : RemovePlaylistRequest = {
        UserId = existingUserId
        Id = existingUserId
    }

    let! removePlaylistResponse = removePlaylist testStore busMock request

    match removePlaylistResponse with
    | Error RemovePlaylistError.PlaylistNotFound ->
        Assert.Pass()

    | other ->
        Assert.Fail(string other)

    let request : RemovePlaylistRequest = {
        UserId = existingUserId
        Id = existingPlaylistId
    }

    let! removePlaylistResponse = removePlaylist testStore busMock request

    match removePlaylistResponse with
    | Ok () ->
        Assert.Pass()

    | other ->
        Assert.Fail(string other)
}
