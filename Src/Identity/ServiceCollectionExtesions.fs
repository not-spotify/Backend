namespace MusicPlayerBackend.Identity

open System.Runtime.CompilerServices
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.DependencyInjection

open MusicPlayerBackend.Common
open MusicPlayerBackend.Persistence.Entities

[<Extension>]
type ServiceCollectionExtensions() =
    [<Extension>]
    static member AddCustomIdentity(sc: IServiceCollection) =
        %sc.AddScoped<IdentityErrorDescriber>()
        %sc.AddScoped<ILookupNormalizer, LookupNormalizer>()
        %sc.AddScoped<IPasswordHasher<User>, PasswordHasher>()
        %sc.AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory<User>>()
        %sc.AddScoped<IUserConfirmation<User>, UserConfirmation>()
        %sc.AddScoped<IUserStore<User>, MusicPlayerBackend.Persistence.FsharpUserStore>()
        %sc.AddScoped<IUserValidator<User>, UserValidator<User>>()
        %sc.AddScoped<UserManager<User>>()
        %sc.AddScoped<SignInManager<User>>()
        sc
