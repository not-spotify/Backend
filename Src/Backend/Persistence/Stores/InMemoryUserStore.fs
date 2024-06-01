module MusicPlayerBackend.Persistence.Stores.InMemoryUserStore

open System.Collections.Generic
open Domain.User
open MusicPlayerBackend.Common

let create () : UserStore =
    let users = Dictionary<UserId, User>()
    { Save =
        fun user ->
            users[user.Id] <- user
            Task.fromResult ^ Ok user

      TryGetByEmail =
        fun email ->
            users
            |> Dictionary.tryFindByValue (fun u -> u.Email = email)
            |> Task.fromResult

      TryGetByUserName =
        fun userName ->
            users
            |> Dictionary.tryFindByValue (fun u -> u.UserName = userName)
            |> Task.fromResult

      TryGetById =
        fun key ->
            users
            |> Dictionary.tryGet key
            |> Task.fromResult

      IsUserWithEmailExist =
          fun email ->
              users |> Seq.exists ^ fun u -> u.Value.Email = email
              |> Task.fromResult

      IsUserWithUserNameExist =
          fun userName ->
              users |> Seq.exists ^ fun u -> u.Value.UserName = userName
              |> Task.fromResult
    }
