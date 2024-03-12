namespace MusicPlayerBackend.Persistence

open System
open Microsoft.EntityFrameworkCore

open MusicPlayerBackend.Common
open MusicPlayerBackend.Persistence
open MusicPlayerBackend.Persistence.Entities
open MusicPlayerBackend.Persistence.Repositories

[<Sealed>]
type FsharpRefreshTokenRepository(dbContext: FsharpAppDbContext) =
    let refreshTokens = dbContext.Set<RefreshToken>()

    member _.Query with get() = refreshTokens.AsQueryable()

    member _.Delete(refreshToken) = refreshTokens.Remove(refreshToken) |> ignore

    member _.Save(refreshToken: RefreshToken) =
        if refreshToken.Id = Guid.Empty && refreshToken.CreatedAt = DateTimeOffset.MinValue then
            refreshToken.CreatedAt <- DateTimeOffset.UtcNow
        elif refreshToken.Id <> Guid.Empty then
            let userEntry = dbContext.Entry(refreshToken)
            match userEntry.State with
            | EntityState.Modified when userEntry.Property(nameof refreshToken.UpdatedAt).IsModified |> not ->
                refreshToken.UpdatedAt <- Some DateTimeOffset.UtcNow
            | EntityState.Detached ->
                raise ^ InvalidOperationException("Can't save detached RefreshToken.")
            | _ -> ()

        refreshTokens.Update(refreshToken)

    member _.TryGetValid(userId, jti, token, ?ct) = task {
        return!
            query {
                for rt in refreshTokens do
                    where(rt.UserId = userId && rt.Jti = jti && rt.Token = token)
                    select rt
            } |> _.TrySingle(ct)
    }
