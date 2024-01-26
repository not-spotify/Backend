using Microsoft.AspNetCore.Identity;
using MusicPlayerBackend.Data.Entities;
using MusicPlayerBackend.Data.Identity.Stores;

namespace MusicPlayerBackend.Services.Identity;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomIdentity(this IServiceCollection sc)
    {
        return sc
            .AddTransient<IdentityErrorDescriber>()
            .AddTransient<ILookupNormalizer, LookupNormalizer>()
            .AddTransient<IPasswordHasher<User>, PasswordHasher>()
            .AddTransient<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory<User>>()
            .AddTransient<IUserConfirmation<User>, UserConfirmation>()
            .AddTransient<IUserStore<User>, UserStore>()
            .AddTransient<IUserValidator<User>, UserValidator>()
            .AddTransient<UserManager<User>, UserManager>()
            .AddTransient<SignInManager<User>, SignInManager>();
    }
}
