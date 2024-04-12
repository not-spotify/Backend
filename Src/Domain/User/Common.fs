module Domain

open System
open System.Runtime.CompilerServices
open System.Threading.Tasks

open Microsoft.FSharp.Core
open Microsoft.FSharp.Linq.RuntimeHelpers
open Microsoft.FSharp.Quotations

open MusicPlayerBackend.Common
open MusicPlayerBackend.Common.ActivePatterns


[<assembly: InternalsVisibleTo("Domain.Tests")>]
do()

type UserId = Guid

type UserName = private UserName of string

[<RequireQualifiedAccess>]
type UserNameError =
    | TooShort
    | TooLong

[<RequireQualifiedAccess>]
module UserName =
    let create (userName: string) =
        match userName with
        | String.Length (LtEq 3) ->
            Error UserNameError.TooShort
        | String.Length (GtEq 20) ->
            Error UserNameError.TooLong
        | _ ->
            Ok ^ UserName userName

    let value (UserName userName) = userName

type Email = private Email of string

[<RequireQualifiedAccess>]
type EmailError =
    | TooShort of minLength: int * actualLength: int
    | TooLong of maxLength: int * actualLength: int

[<RequireQualifiedAccess>]
module Email =

    let MinLength = 3
    let MaxLength = 20

    let create (email: string) =
        match email with
        | String.Length (LtEq MinLength as actualLength) ->
            Error (EmailError.TooShort (MinLength, actualLength))
        | String.Length (GtEq MaxLength as actualLength) ->
            Error (EmailError.TooLong (MaxLength, actualLength))
        | _ ->
            Ok (Email email)

type Password = private Password of string

[<RequireQualifiedAccess>]
type PasswordError =
    | TooShort of minLength: int * actualLength: int
    | TooLong of maxLength: int * actualLength: int

[<RequireQualifiedAccess>]
module Password =

    let MinLength = 3
    let MaxLength = 20

    let create (password: string) =
        match password with
        | String.Length (LtEq MinLength as actualLength) ->
            Error (PasswordError.TooShort (MinLength, actualLength))
        | String.Length (GtEq MaxLength as actualLength) ->
            Error (PasswordError.TooLong (MaxLength, actualLength))
        | _ ->
            Ok (Password password)

    let value (Password password) = password

type User = {
    Id: UserId
    UserName: UserName
    Email: Email
    Password: Password
    CreatedAt: DateTimeOffset
    UpdatedAt: DateTimeOffset option
}

type CreateUserRequest = {
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
    | UserName
    | DuplicateEmail

type CreateUserResponse =
    | Created of User
    | Failed of CreateUserFailed list
    | ValidationFailed of UserFieldValidationFailed list

[<NoComparison; NoEquality>]
type UserStore = {
    Save: User -> Task<CreateUserResponse>
    TryGetByEmail: Email -> Task<User option>
    TryGetByUserName: UserName -> Task<User option>
}

type CreateUser = CreateUserRequest -> Task<CreateUserResponse>

let private eval = LeafExpressionConverter.EvaluateQuotation

type FieldName = private FieldName of string

module FieldName =
    let create (p:  Reflection.PropertyInfo) =
        FieldName p.Name

let getFieldNameAndValue<'TField> (exp: Expr<'TField>) : (FieldName * 'TField) option =
    match exp, eval exp |> tryUcast with
    | Patterns.PropertyGet (_, propertyInfo, _), Some value ->
        Some (FieldName.create propertyInfo, value)
    | _ ->
        None

module Reflection =
    open Microsoft.FSharp.Reflection

    let [<Literal>] InvalidTypeMessage = "Unxpected Union type parameter"

    [<RequireQualifiedAccess>]
    type ReflectionError =
        | NotUnionType
        | FieldNotFound

    let createEnumByName<'TUnion, 'T> (name: string) (value: 'T) =
        match typeof<'T> with
        | FSharpType.Union ->
            let cases = FSharpType.GetUnionCases(typeof<'TUnion>)
            let case = cases |> Array.tryFind ^ fun c -> c.Name = name

            match case with
            | None ->
                Error ReflectionError.FieldNotFound
            | Some case ->
                FSharpValue.MakeUnion(case, [| box value |])
                |> ucast<obj, 'TUnion>
                |> Ok
        | _ ->
            Error ReflectionError.NotUnionType

type TypeValidationBuilder<'T>() =
    member this.Bind(m, f) =
        m

    member this.Return(v) =
        v

    member this.ReturnFrom(m: (string -> Result<UserName, UserNameError>) * Expr<string>) =
        let validator, expression = m
        match getFieldNameAndValue expression with
        | Some (fieldName, value) ->
            let res = validator value
            // TODO: Create enum (DU) with name = fieldName
            ()
        | _ -> failwith "TODO"
        m

    member this.Zero() =
        []

let typeValidation<'T> = TypeValidationBuilder<'T>()

let validateRequest (request: CreateUserRequest) = typeValidation<UserFieldValidationFailed> {
    let! userName = UserName.create, <@ request.UserName @>
    ()
}

let createUser (userStore: UserStore) : CreateUser =

    fun createUser -> task {
        return CreateUserResponse.Failed []
    }
