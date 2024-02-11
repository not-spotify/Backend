namespace MusicPlayerBackend.Persistence.Entities

open System

type RefreshTokenId = Guid
type Jti = Guid

type RefreshToken = {
    Id: RefreshTokenId
    Jti: Jti
    Token: Guid
    Revoked: bool
    ValidDue: DateTimeOffset
    CreatedAt: DateTimeOffset
    UpdatedAt: DateTimeOffset voption
}
