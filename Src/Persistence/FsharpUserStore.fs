namespace MusicPlayerBackend.Persistence

open System.Threading.Tasks
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.Logging

open MusicPlayerBackend.Common
open MusicPlayerBackend.Persistence.Entities

type IUserStore = inherit IUserPasswordStore<User> inherit IUserEmailStore<User>

[<Sealed>]
type FsharpUserStore(
    logger: ILogger<FsharpUserStore>,
    lookupNormalizer: ILookupNormalizer,
    userRepository: FsharpUserRepository,
    unitOfWork: FsharpUnitOfWork) as this =

     let self = (this :> IUserStore)

     interface IUserStore with
        member this.CreateAsync(user, cancellationToken) = task {
            cancellationToken.ThrowIfCancellationRequested()
            try
                let normalizedUserName = lookupNormalizer.NormalizeName(user.UserName)
                let! userByNormalizedUserName = self.FindByNameAsync(normalizedUserName, cancellationToken)

                let normalizedEmail = lookupNormalizer.NormalizeEmail(user.Email);
                let! userByNormalizedEmail = self.FindByEmailAsync(normalizedEmail, cancellationToken)

                if userByNormalizedUserName |> Option.ofUncheckedObj |> Option.isSome then
                    return IdentityResult.Failed(IdentityErrorDescriber().DuplicateUserName(normalizedUserName))
                elif userByNormalizedEmail |> Option.ofUncheckedObj |> Option.isSome then
                    return IdentityResult.Failed(IdentityErrorDescriber().DuplicateEmail(user.Email))
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
            cancellationToken.ThrowIfCancellationRequested()
            userRepository.Delete(user)
            IdentityResult.Success
            |> Task.fromResult

        member this.Dispose() =
            logger.LogDebug("Called Dispose")

        member this.FindByEmailAsync(normalizedEmail, cancellationToken) = task {
            cancellationToken.ThrowIfCancellationRequested()
            return!
                userRepository
                    .TryGetByNormalizedEmail(normalizedEmail, cancellationToken)
                |> TaskOption.toUncheckedObj
        }

        member this.FindByIdAsync(rawUserId, cancellationToken) = task {
            cancellationToken.ThrowIfCancellationRequested()
            let userId = System.Guid.TryParse(rawUserId) |> Option.ofTry

            match userId with
            | None ->
                logger.LogWarning("Tried to find {userId}. Can't cast to Guid", rawUserId)
                return Unchecked.defaultof<_>
            | Some userId ->
                return!
                    userRepository
                        .TryGetById(userId, cancellationToken)
                    |> TaskOption.toUncheckedObj
        }

        member this.FindByNameAsync(normalizedUserName, cancellationToken) = task {
            cancellationToken.ThrowIfCancellationRequested()
            return!
                userRepository
                    .TryGetByNormalizedEmail(normalizedUserName, cancellationToken)
                |> TaskOption.toUncheckedObj
        }

        member this.GetEmailAsync(user, cancellationToken) =
            cancellationToken.ThrowIfCancellationRequested()
            user.Email
            |> Task.FromResult

        member this.GetEmailConfirmedAsync(_user, cancellationToken) = // TODO: #8 Implement user account verification
            cancellationToken.ThrowIfCancellationRequested()
            Task.bTrue

        member this.GetNormalizedEmailAsync(user, cancellationToken) =
            cancellationToken.ThrowIfCancellationRequested()
            user.NormalizedEmail
            |> Task.FromResult

        member this.GetNormalizedUserNameAsync(user, cancellationToken) =
            cancellationToken.ThrowIfCancellationRequested()
            Task.FromResult(user.NormalizedUserName)

        member this.GetPasswordHashAsync(user, cancellationToken) = // TODO: #9 Implement user password hashing
            cancellationToken.ThrowIfCancellationRequested()
            Task.FromResult(user.HashedPassword)

        member this.GetUserIdAsync(user, cancellationToken) =
            cancellationToken.ThrowIfCancellationRequested()
            Task.FromResult(string user.Id)

        member this.GetUserNameAsync(user, cancellationToken) =
            cancellationToken.ThrowIfCancellationRequested()
            user.UserName
            |> Task.FromResult

        member this.HasPasswordAsync(_user, cancellationToken) =
            cancellationToken.ThrowIfCancellationRequested()
            Task.bTrue

        member this.SetEmailAsync(user, email, cancellationToken) =
            cancellationToken.ThrowIfCancellationRequested()
            user.Email <- email
            Task.completed

        member this.SetEmailConfirmedAsync(_user, _confirmed, cancellationToken) =
            cancellationToken.ThrowIfCancellationRequested()
            Task.bTrue

        member this.SetNormalizedEmailAsync(user, normalizedEmail, cancellationToken) =
            cancellationToken.ThrowIfCancellationRequested()
            user.NormalizedUserName <- normalizedEmail
            Task.completed

        member this.SetNormalizedUserNameAsync(user, normalizedName, cancellationToken) =
            cancellationToken.ThrowIfCancellationRequested()
            user.NormalizedUserName <- normalizedName
            Task.completed

        member this.SetPasswordHashAsync(user, passwordHash, cancellationToken) =
            cancellationToken.ThrowIfCancellationRequested()
            user.HashedPassword <- passwordHash
            Task.completed

        member this.SetUserNameAsync(user, userName, cancellationToken) =
            cancellationToken.ThrowIfCancellationRequested()
            user.UserName <- userName
            Task.completed

        member this.UpdateAsync(user, cancellationToken) =
            cancellationToken.ThrowIfCancellationRequested()
            // TODO: Implement ConcurrencyStamp for User
            userRepository.Save(user) |> ignore
            IdentityResult.Success
            |>Task.fromResult
