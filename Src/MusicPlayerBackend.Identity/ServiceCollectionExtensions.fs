namespace MusicPlayerBackend.Identity.ServiceCollectionExtensions

[<AutoOpen>]
module internal Extension =
    open Microsoft.AspNetCore.Identity
    open Microsoft.Extensions.DependencyInjection

    open MusicPlayerBackend.Identity
    open MusicPlayerBackend.Common
    open MusicPlayerBackend.Data.Entities
    open MusicPlayerBackend.Data.Identity.Stores

    type IServiceCollection with
        member sc.AddCustomIdentity() =
            %sc.AddScoped<IdentityErrorDescriber>()
            %sc.AddScoped<ILookupNormalizer, LookupNormalizer>()
            %sc.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>()
            %sc.AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory<User>>()
            %sc.AddScoped<IUserConfirmation<User>, UserConfirmation>()
            %sc.AddScoped<IUserStore<User>, UserStore>()
            %sc.AddScoped<IUserValidator<User>, UserValidator<User>>()
            %sc.AddScoped<UserManager<User>>()
            %sc.AddScoped<SignInManager<User>>()
            sc
