namespace MusicPlayerBackend.Host.Models

open System
open Domain.Shared
open System.ComponentModel.DataAnnotations

[<CLIMutable>]
type RegisterRequest = {
    [<Required>]
    [<StringLength(UserName.MaxLength, MinimumLength = UserName.MinLength)>]
    UserName: string

    [<Required>]
    [<DataType(DataType.EmailAddress)>]
    [<StringLength(Email.MaxLength, MinimumLength = Email.MinLength)>]
    Email: string

    [<Required>]
    [<DataType(DataType.Password)>]
    [<StringLength(Password.MaxLength, MinimumLength = Email.MinLength)>]
    Password: string
}

[<CLIMutable>]
type UserResponse = {
    [<Required>]
    Id: Guid

    [<Required>]
    [<StringLength(UserName.MaxLength, MinimumLength = UserName.MinLength)>]
    UserName: string

    [<Required>]
    RegisteredAt: DateTimeOffset
}
