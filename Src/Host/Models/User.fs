namespace MusicPlayerBackend.Host.Models

open System

type UserId = Guid

type RegisterRequest = {
    /// <example>metauser</example>
    UserName: string

    /// <example>meta@mail.local</example>
    Email: string

    /// <example>somesecurepassword</example>
    Password: string
}

type LoginRequest = {
    /// <example>meta@mail.local</example>
    Email: string

    /// <example>somesecurepassword</example>
    Password: string
}

type RefreshTokenRequest = {
    Jti: Guid
    RefreshToken: Guid
    Id: UserId
}

type TokenResponse = {
    Id: UserId
    JwtBearer: string
    RefreshToken: string
    RefreshTokenValidDue: DateTime
    JwtBearerValidDue: DateTime
}

type User = {
    Id: UserId
    UserName: string
    Email: string
}
