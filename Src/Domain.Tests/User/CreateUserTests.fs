module Domain.Tests.User.CreateUserTests

open System
open Domain.Shared
open NUnit.Framework

open MusicPlayerBackend.Common
open Contracts.Shared
open Domain.User

let testStore (users: User seq) : UserStore =
    let users = ResizeArray(users)

    let save user = task {
        return Ok user
    }

    let tryGetByEmail email = task {
        return users |> Seq.tryFind ^ fun u -> u.Email = email
    }

    let tryGetByUserName (userName: UserName) = task {
        return users |> Seq.tryFind ^ fun u -> u.UserName = userName
    }

    let tryGetById (userId: UserId) = task {
        return users |> Seq.tryFind ^ fun u -> u.Id = userId
    }

    let isUserWithEmailExist (email: Email) = task {
        return users |> Seq.exists ^ fun u -> u.Email = email
    }

    let isUserWithUserNameExist (userName: UserName) = task {
        return users |> Seq.exists ^ fun u -> u.UserName = userName
    }

    { Save = save
      TryGetByEmail = tryGetByEmail
      TryGetByUserName = tryGetByUserName
      TryGetById = tryGetById
      IsUserWithEmailExist = isUserWithEmailExist
      IsUserWithUserNameExist = isUserWithUserNameExist }

[<Test>]
let ``Try create user with duplicated email`` () = task {
    let testStore = testStore [|
                                 { UserName = Domain.Shared.UserName "Foo"
                                   Email = Domain.Shared.Email "me@me.me"
                                   Password = Domain.Shared.Password "SomeCoolPassword"
                                   CreatedAt = DateTimeOffset.UtcNow
                                   UpdatedAt = None
                                   Id = Id.create() }
                              |]

    let busMock : Bus = {
        Publish = fun _ -> Task.completed
    }

    let request = {
        UserName = "MetaUser"
        Email = "me@me.me"
        Password = "SomeValidPassword"
    }

    let! createUserResponse = request |> createUser testStore busMock

    match createUserResponse with
    | Error (Failed [ CreateUserFailed.DuplicateEmail ]) ->
        Assert.Pass()

    | other ->
        Assert.Fail(string other)
}

