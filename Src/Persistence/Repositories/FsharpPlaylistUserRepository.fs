namespace MusicPlayerBackend.Persistence

open System
open Microsoft.EntityFrameworkCore

open MusicPlayerBackend.Common
open MusicPlayerBackend.Persistence
open MusicPlayerBackend.Persistence.Entities
open MusicPlayerBackend.Persistence.Repositories

[<Sealed>]
type FsharpPlaylistUserPermissionRepository(dbContext: FsharpAppDbContext) =
    let permissions = dbContext.Set<PlaylistUserPermission>()

    member _.Query with get() = permissions.AsQueryable()

    member _.Delete(permission) = permissions.Remove(permission) |> ignore

    member _.Save(permission: PlaylistUserPermission) =
        if permission.UserId = Guid.Empty && permission.PlaylistId = Guid.Empty && permission.CreatedAt = DateTimeOffset.MinValue then
            permission.CreatedAt <- DateTimeOffset.UtcNow
        elif permission.UserId <> Guid.Empty && permission.PlaylistId <> Guid.Empty then
            let userEntry = dbContext.Entry(permission)
            match userEntry.State with
            | EntityState.Modified when userEntry.Property(nameof permission.UpdatedAt).IsModified |> not ->
                permission.UpdatedAt <- Some DateTimeOffset.UtcNow
            | EntityState.Detached ->
                raise ^ InvalidOperationException("Can't save detached permission.")
            | _ -> ()

        permissions.Update(permission)
