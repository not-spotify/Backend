namespace MusicPlayerBackend.Host.Controllers

open System.Net.Mime
open Domain.User
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc

open MusicPlayerBackend.Host.Models

[<ApiController>]
[<Consumes(MediaTypeNames.Application.Json)>]
[<Produces(MediaTypeNames.Application.Json)>]
[<ProducesResponseType(StatusCodes.Status400BadRequest)>]
[<Route("api/v0/[controller]/[action]")>]
type AuthController() as this =

    inherit ControllerBase()

    /// Register new user
    [<HttpPost(Name = "Register")>]
    [<ProducesResponseType(typeof<UserResponse>, StatusCodes.Status200OK)>]
    [<ProducesResponseType(typeof<InvalidRequestResponse>, StatusCodes.Status400BadRequest)>]
    member _.Register(request: RegisterRequest, [<FromServices>] handler: CreateUser) = task {

        let command : CreateUserCommand = {
            UserName = request.UserName
            Email = request.Email
            Password = request.Password
        }

        match! handler command with
        | Ok user ->
            let response : UserResponse = {
                Id = user.Id
                UserName = string user.UserName
                RegisteredAt = user.CreatedAt
            }

            return this.Ok(response) :> IActionResult

        | Error error ->
            match error with
            | Failed userFailed ->
                return this.BadRequest({ Message = string userFailed })
            | ValidationFailed failed ->
                return this.BadRequest({ Message = string failed })
    }
