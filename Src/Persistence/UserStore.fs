namespace MusicPlayerBackend.Persistence

open System.Threading.Tasks
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.Logging

open MusicPlayerBackend.Common

type IUserStore = inherit IUserPasswordStore<Entities.User.User> inherit IUserEmailStore<Entities.User.User>

type UserStore(
    logger: ILogger<UserStore>,
    lookupNormalizer: ILookupNormalizer,
    userRepository: FsharpUserRepository,
    unitOfWork: FSharpUnitOfWork) as this =

     let self = (this :> IUserStore)

     interface IUserStore with
         member this.CreateAsync(user, cancellationToken) = task {
            try
                let normalizedUserName = lookupNormalizer.NormalizeName(user.UserName)
                let! userByNormalizedUserName = self.FindByNameAsync(normalizedUserName, cancellationToken)

                let normalizedEmail = lookupNormalizer.NormalizeEmail(user.Email);
                let! userByNormalizedEmail = self.FindByEmailAsync(normalizedEmail, cancellationToken)

                if userByNormalizedUserName |> Option.ofUncheckedObj |> Option.isSome then
                    return IdentityResult.Failed((IdentityErrorDescriber()).DuplicateUserName(normalizedUserName))
                elif userByNormalizedEmail |> Option.ofUncheckedObj |> Option.isSome then
                    return IdentityResult.Failed((IdentityErrorDescriber()).DuplicateEmail(user.Email))
                else
                    user.NormalizedUserName <- normalizedUserName
                    user.NormalizedEmail <- normalizedEmail
                    do! unitOfWork.SaveChanges(cancellationToken)

                    return IdentityResult.Success
             with e ->
                logger.LogError(e, "Failed to create user: {user}", user)
                return IdentityResult.Failed()
         }

         member this.DeleteAsync(user, cancellationToken) =
                userRepository.Delete(user);
                Task.FromResult(IdentityResult.Success)
         member this.Dispose() = failwith "todo"
         member this.FindByEmailAsync(normalizedEmail, cancellationToken) = failwith "todo"
         member this.FindByIdAsync(userId, cancellationToken) = failwith "todo"
         member this.FindByNameAsync(normalizedUserName, cancellationToken) = failwith "todo"
         member this.GetEmailAsync(user, cancellationToken) = failwith "todo"
         member this.GetEmailConfirmedAsync(user, cancellationToken) = failwith "todo"
         member this.GetNormalizedEmailAsync(user, cancellationToken) = failwith "todo"
         member this.GetNormalizedUserNameAsync(user, cancellationToken) = failwith "todo"
         member this.GetPasswordHashAsync(user, cancellationToken) = failwith "todo"
         member this.GetUserIdAsync(user, cancellationToken) =
             Task.FromResult(user.Id.ToString())
         member this.GetUserNameAsync(user, cancellationToken) = failwith "todo"
         member this.HasPasswordAsync(user, cancellationToken) = failwith "todo"
         member this.SetEmailAsync(user, email, cancellationToken) = failwith "todo"
         member this.SetEmailConfirmedAsync(user, confirmed, cancellationToken) = failwith "todo"
         member this.SetNormalizedEmailAsync(user, normalizedEmail, cancellationToken) = failwith "todo"
         member this.SetNormalizedUserNameAsync(user, normalizedName, cancellationToken) = failwith "todo"
         member this.SetPasswordHashAsync(user, passwordHash, cancellationToken) = failwith "todo"
         member this.SetUserNameAsync(user, userName, cancellationToken) = failwith "todo"
         member this.UpdateAsync(user, cancellationToken) = failwith "todo"
