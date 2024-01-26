using Microsoft.AspNetCore.Identity;
using MusicPlayerBackend.Data.Entities;
using MusicPlayerBackend.Data.Identity.Stores;

namespace MusicPlayerBackend.Services.Identity;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomIdentity(this IServiceCollection sc)
    {
        return sc
            .AddScoped<IdentityErrorDescriber>()
            .AddScoped<ILookupNormalizer, LookupNormalizer>()
            .AddScoped<IPasswordHasher<User>, PasswordHasher>()
            .AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory<User>>()
            .AddScoped<IUserConfirmation<User>, UserConfirmation>()
            .AddScoped<IUserStore<User>, UserStore>()
            .AddScoped<IUserValidator<User>, UserValidator>()
            .AddScoped<UserManager<User>, UserManager>()
            .AddScoped<SignInManager<User>, SignInManager>();
    }
}
