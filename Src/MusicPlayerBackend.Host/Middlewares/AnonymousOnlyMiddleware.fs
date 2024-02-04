namespace MusicPlayerBackend.Host

open System.Net.Mime
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Http.Features
open System.Text.Json
open Microsoft.AspNetCore.Http.Json
open Microsoft.Extensions.Options
open Microsoft.Extensions.DependencyInjection

open MusicPlayerBackend.TransferObjects

[<Sealed>]
type AnonymousOnlyMiddleware(next: RequestDelegate) =
    member _.InvokeAsync(context: HttpContext) = task {
        do! next.Invoke(context)

        let endpoint = context.Features.Get<IEndpointFeature>().Endpoint
        let attribute = endpoint.Metadata.GetMetadata<AllowAnonymousOnlyAttribute>() |> ValueOption.ofObj

        if ValueOption.isSome attribute && context.User.Identity.IsAuthenticated then
            context.Response.ContentType <- MediaTypeNames.Application.Json

            let jsonOptions = context.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions
            let response = UnauthorizedResponse(Error = "You should be unauthorized")

            do! context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions))
    }
