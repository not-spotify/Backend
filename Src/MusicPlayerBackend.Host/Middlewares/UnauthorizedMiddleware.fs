namespace MusicPlayerBackend.Host

open System.Net.Mime;
open System.Text.Json;
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Http.Json;
open Microsoft.Extensions.Options
open Microsoft.Extensions.DependencyInjection
open MusicPlayerBackend.TransferObjects

type UnauthorizedMiddleware(next: RequestDelegate) =
    member _.InvokeAsync(context: HttpContext) = task {
        do! next.Invoke(context)

        if context.Response.HasStarted = false && context.Response.StatusCode <> StatusCodes.Status401Unauthorized then
            context.Response.ContentType <- MediaTypeNames.Application.Json

            let jsonOptions = context.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions
            let response = UnauthorizedResponse(Error = "Unauthorized. Refresh token or authorize.")

            do! context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions))
    }
