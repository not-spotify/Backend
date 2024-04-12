namespace MusicPlayerBackend.Host.Models

open System

type UserId = Guid

[<CLIMutable>]
type RegisterRequest = {
    /// <example>metauser</example>
    UserName: string

    /// <example>meta@mail.local</example>
    Email: string

    /// <example>somesecurepassword</example>
    Password: string
}

[<CLIMutable>]
type LoginRequest = {
    /// <example>meta@mail.local</example>
    Email: string

    /// <example>somesecurepassword</example>
    Password: string
}

[<CLIMutable>]
type RefreshTokenRequest = {
    Jti: Guid
    RefreshToken: Guid
    Id: UserId
}

[<CLIMutable>]
type TokenResponse = {
    Id: UserId
    JwtBearer: string
    RefreshToken: string
    RefreshTokenValidDue: DateTime
    JwtBearerValidDue: DateTime
}

[<CLIMutable>]
type User = {
    Id: UserId
    UserName: string
    Email: string
}
