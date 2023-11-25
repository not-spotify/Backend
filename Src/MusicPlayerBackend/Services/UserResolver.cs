using Microsoft.AspNetCore.Identity;
using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Services;

public interface IUserResolver
{
    Task<User> GetUserAsync();
    Task<Guid> GetUserIdAsync();
}

public sealed class UserResolver(IHttpContextAccessor httpContextAccessor, UserManager<User> userManager) : IUserResolver
{
    public async Task<User> GetUserAsync()
    {
        var claimsPrincipal = httpContextAccessor.HttpContext!.User;
        var user = await userManager.GetUserAsync(claimsPrincipal);

        if (user == default)
            throw new NullReferenceException($"Can't find user. Claims: {claimsPrincipal}");

        return user;
    }

    public async Task<Guid> GetUserIdAsync()
    {
        var claimsPrincipal = httpContextAccessor.HttpContext!.User;
        var user = await userManager.GetUserAsync(claimsPrincipal);

        if (user == default)
            throw new NullReferenceException($"Can't find user. Claims: {claimsPrincipal}");

        return user.Id;
    }
}
