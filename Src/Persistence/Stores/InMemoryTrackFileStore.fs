module MusicPlayerBackend.Persistence.Stores.InMemoryTrackFileStore

open System
open System.Collections.Generic
open System.IO
open Domain.Track

let create () : TrackFileStore =
    let tracks = Dictionary<FileUri, byte[]>()

    { Save =
        fun stream -> task {
            use ms = new MemoryStream()
            stream.CopyTo(ms)

            let fileUri = Guid.NewGuid().ToString()
            tracks[fileUri] <- ms.ToArray()

            return fileUri
        }

      Get =
          fun fileUri -> task {
              let ms = new MemoryStream(tracks[fileUri])
              ms.Position <- 0

              return Ok ms
          }
    }
