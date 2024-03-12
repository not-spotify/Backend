namespace MusicPlayerBackend.Identity

open Microsoft.AspNetCore.Identity

open MusicPlayerBackend.Persistence.Entities

type PasswordHasher() =
    interface IPasswordHasher<User> with
        member this.HashPassword(_user, password) =
            password

        member this.VerifyHashedPassword(_user, hashedPassword, providedPassword) =
            if hashedPassword = providedPassword then
                PasswordVerificationResult.Success
            else
                PasswordVerificationResult.Failed
