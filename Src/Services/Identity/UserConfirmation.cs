using Microsoft.AspNetCore.Identity;
using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Services.Identity;

public sealed class UserConfirmation : IUserConfirmation<User>
{
    public Task<bool> IsConfirmedAsync(UserManager<User> manager, User user)
    {
        return Task.FromResult(true);
    }
}
