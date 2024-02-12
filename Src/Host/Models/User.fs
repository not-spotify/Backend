module MusicPlayerBackend.Host.Models.User

open System

type UserId = Guid

type RegisterRequest = {
    UserName: string
    Email: string
    Password: string
}

type RegisterResponse = {
    Id: UserId
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
    RefreshTokenValidDue: DateTime // TODO: offset?
    JwtBearerValidDue: DateTime
}

type UserResponse = {
    Id: UserId
    UserName: string
    Email: string
}

