module MusicPlayerBackend.Persistence.Stores.InMemoryPlaylistStore

open System.Collections.Generic
open Domain.Playlist
open MusicPlayerBackend.Common

let create () : PlaylistStore =
    let playlists = Dictionary<PlaylistId, Playlist>()

    { GetByUserId =
        fun userId -> task {
            return playlists
                   |> Seq.filter ^ fun p -> p.Value.UserId = userId
                   |> Seq.map _.Value
                   |> Array.ofSeq
        }
      TryGetByName = fun (name, userId) -> task {
            return playlists
                   |> Dictionary.tryFindByValue ^ fun p -> p.UserId = userId && p.Name = name
         }

      IsPlaylistExist = fun (name, userId) -> task {
          return playlists |> Seq.exists ^ fun p -> p.Value.UserId = userId && p.Value.Name = name
      }

      TryGetById = fun id -> task {
          return playlists.TryGetValue(id) |> Option.ofTry
      }

      Save = fun playlist -> task {
          playlists[playlist.Id] <- playlist
          return Ok playlist
      }
    }
