using Microsoft.AspNetCore.Identity;
using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Services;

public interface IUserProvider
{
    Task<User> GetUserAsync();
    Task<User?> GetUserOrDefaultAsync();
    Task<Guid> GetUserIdAsync();
    Task<Guid?> GetUserIdOrDefaultAsync();
}

public sealed class UserProvider(IHttpContextAccessor httpContextAccessor, UserManager<User> userManager) : IUserProvider
{
    public async Task<User> GetUserAsync()
    {
        var claimsPrincipal = httpContextAccessor.HttpContext!.User;
        var user = await userManager.GetUserAsync(claimsPrincipal);

        if (user == default)
            throw new NullReferenceException($"Can't find user. Claims: {claimsPrincipal}");

        return user;
    }

    public async Task<User?> GetUserOrDefaultAsync()
    {
        var claimsPrincipal = httpContextAccessor.HttpContext?.User;
        if (claimsPrincipal == default)
            return default;

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

    public async Task<Guid?> GetUserIdOrDefaultAsync()
    {
        var claimsPrincipal = httpContextAccessor.HttpContext?.User;
        if (claimsPrincipal == default)
            return default;

        var user = await userManager.GetUserAsync(claimsPrincipal);

        if (user == default)
            throw new NullReferenceException($"Can't find user. Claims: {claimsPrincipal}");

        return user.Id;
    }
}
