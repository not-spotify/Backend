namespace MusicPlayerBackend.Persistence

open System
open Microsoft.EntityFrameworkCore

open MusicPlayerBackend.Common
open MusicPlayerBackend.Persistence
open MusicPlayerBackend.Persistence.Entities

[<Sealed>]
type FsharpAlbumRepository(dbContext: FsharpAppDbContext) =
    let albums = dbContext.Set<Album>()

    member _.Query with get() = albums.AsQueryable()

    member _.Delete(refreshToken) = albums.Remove(refreshToken) |> ignore

    member _.Save(album: Album) =
        if album.Id = Guid.Empty && album.CreatedAt = DateTimeOffset.MinValue then
            album.CreatedAt <- DateTimeOffset.UtcNow
        elif album.Id <> Guid.Empty then
            let userEntry = dbContext.Entry(album)
            match userEntry.State with
            | EntityState.Modified when userEntry.Property(nameof album.UpdatedAt).IsModified |> not ->
                album.UpdatedAt <- ValueSome DateTimeOffset.UtcNow
            | EntityState.Detached ->
                raise ^ InvalidOperationException("Can't save detached RefreshToken.")
            | _ -> ()

        albums.Update(album)
