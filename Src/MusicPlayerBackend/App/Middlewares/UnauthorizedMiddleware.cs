using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using MusicPlayerBackend.TransferObjects;

namespace MusicPlayerBackend.App.Middlewares;

internal sealed class UnauthorizedMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        if (context.Response.StatusCode != StatusCodes.Status401Unauthorized)
            return;

        if (context.Response.HasStarted)
            return;

        context.Response.ContentType = MediaTypeNames.Application.Json;

        var jsonOptions = context.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;
        var response = new UnauthorizedResponse { Error = "Unauthorized. Refresh token or authorize." };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
