namespace MusicPlayerBackend.Identity

open Microsoft.AspNetCore.Identity

open MusicPlayerBackend.Data.Entities

type UserManager(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger) =
    inherit UserManager<User>(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
