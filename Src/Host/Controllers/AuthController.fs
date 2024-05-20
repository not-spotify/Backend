namespace MusicPlayerBackend.Host.Controllers

open System.Net.Mime
open Domain.User
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc

[<ApiController>]
[<Consumes(MediaTypeNames.Application.Json)>]
[<Produces(MediaTypeNames.Application.Json)>]
[<ProducesResponseType(StatusCodes.Status400BadRequest)>]
[<Route("api/v0/[controller]/[action]")>]
type AuthController() as this =

    inherit ControllerBase()

    /// Register new user
    [<HttpPost(Name = "Register")>]
    member _.Register(test, [<FromServices>] handler: CreateUser) = task {
        let! x = handler test
        return this.Ok(x)
    }
