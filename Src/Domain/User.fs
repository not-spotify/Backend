module Domain.User

open System
open System.Runtime.CompilerServices
open System.Threading.Tasks

open Contracts.Shared
open Domain.Shared
open Microsoft.FSharp.Core

open FsToolkit.ErrorHandling

[<assembly: InternalsVisibleTo("Domain.Tests")>]
do()

type UserId = Id

type User = {
    Id: UserId
    UserName: UserName
    Email: Email
    Password: Password
    CreatedAt: DateTimeOffset
    UpdatedAt: DateTimeOffset option
}

type CreateUserCommand = {
    UserName: string
    Email: string
    Password: string
}

type UserFieldValidationFailed =
    | Email of EmailError
    | UserName of UserNameError
    | Password of PasswordError
    | UnknownError of string

type CreateUserFailed =
    | DuplicateUserName
    | DuplicateEmail

type CreateUserError =
    | Failed of CreateUserFailed list
    | ValidationFailed of UserFieldValidationFailed list

[<NoComparison; NoEquality>]
type UserStore = {
    Save: User -> Task<Result<User, CreateUserError>>
    TryGetById: UserId -> Task<User option>
    IsUserWithEmailExist: Email -> Task<bool>
    IsUserWithUserNameExist: UserName -> Task<bool>
    TryGetByEmail: Email -> Task<User option>
    TryGetByUserName: UserName -> Task<User option>
}

type CreateUser = CreateUserCommand -> TaskResult<User, CreateUserError>

let createUserInternal (request: CreateUserCommand) =
    validation {
        let! email = request.Email |> Email.create |> Result.mapError Email
        and! userName = request.UserName |> UserName.create |> Result.mapError UserName
        and! password = request.Password |> Password.create |> Result.mapError Password

        return { Id = Id.create()
                 UserName = userName
                 Email = email
                 Password = password
                 CreatedAt = DateTimeOffset.UtcNow
                 UpdatedAt = None }
    }

let createUser (userStore: UserStore) (bus: Bus) : CreateUser =
    fun request -> taskResult {
        let! newUser = createUserInternal request
                       |> Result.mapError CreateUserError.ValidationFailed

        do! userStore.IsUserWithEmailExist(newUser.Email)
            |> TaskResult.requireFalse (CreateUserError.Failed [ CreateUserFailed.DuplicateEmail ])

        do! userStore.IsUserWithUserNameExist(newUser.UserName)
            |> TaskResult.requireFalse (CreateUserError.Failed [ CreateUserFailed.DuplicateUserName ])

        let! user = userStore.Save(newUser)
        do! bus.Publish(user)

        return user
    }
