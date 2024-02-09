using System;
using System.Threading;
using System.Threading.Tasks;
using MusicPlayerBackend.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Data.Identity.Stores;


public sealed class UserStore(
    ILogger<UserStore> logger,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILookupNormalizer lookupNormalizer,
    IPasswordHasher<User> passwordHasher) : IUserPasswordStore<User>, IUserEmailStore<User>
{
    public void Dispose()
    {
        logger.LogDebug("Called Dispose");
    }

    public Task<string> GetUserIdAsync(User user, CancellationToken ct)
    {
        return Task.FromResult(user.Id.ToString());
    }

    public Task<string?> GetUserNameAsync(User user, CancellationToken ct)
    {
        return Task.FromResult(user.Email)!;
    }

    public Task SetUserNameAsync(User user, string? userName, CancellationToken ct)
    {
        if (userName == default)
            throw new ArgumentException("Can't set username to null", nameof(userName));

        user.Email = userName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken ct)
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken ct)
    {
        if (normalizedName == default)
            throw new ArgumentException("Can't set username to null", nameof(normalizedName));

        user.NormalizedEmail = normalizedName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> CreateAsync(User user, CancellationToken ct)
    {
        try
        {
            var normalizedUserName = lookupNormalizer.NormalizeName(user.UserName);
            if (normalizedUserName != default)
            {
                var userByNormalizedUserName = await FindByNameAsync(normalizedUserName, ct);
                if (userByNormalizedUserName != default)
                    return IdentityResult.Failed(new IdentityErrorDescriber().DuplicateUserName(normalizedUserName));
            }

            var normalizedEmail = lookupNormalizer.NormalizeEmail(user.Email);
            var userByNormalizedEmail = await FindByEmailAsync(normalizedEmail, ct);
            if (userByNormalizedEmail != default)
                return IdentityResult.Failed(new IdentityErrorDescriber().DuplicateEmail(user.Email));

            user.NormalizedUserName = normalizedUserName;
            user.NormalizedEmail = normalizedEmail;
            await unitOfWork.SaveChangesAsync(ct);

            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to create user: {user}", user);
            return IdentityResult.Failed();
        }
    }

    public async Task<IdentityResult> UpdateAsync(User user, CancellationToken ct)
    {
        try
        {
            user.NormalizedUserName = lookupNormalizer.NormalizeName(user.UserName);
            user.NormalizedEmail = lookupNormalizer.NormalizeEmail(user.Email);

            return await Task.FromResult(IdentityResult.Success);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to update user: {user}", user);
            return await Task.FromResult(IdentityResult.Failed());
        }
    }

    public Task<IdentityResult> DeleteAsync(User user, CancellationToken ct)
    {
        try
        {
            userRepository.Delete(user);
            return Task.FromResult(IdentityResult.Success);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to delete user: {user}", user);
            return Task.FromResult(IdentityResult.Failed());
        }
    }

    public async Task<User?> FindByIdAsync(string userId, CancellationToken ct)
    {
        return await userRepository.GetByIdOrDefaultAsync(Guid.Parse(userId), ct);
    }

    public async Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken ct)
    {
        return await userRepository.FindByNormalizedEmailOrDefaultAsync(normalizedUserName, ct);
    }

    public Task SetPasswordHashAsync(User user, string? passwordHash, CancellationToken ct)
    {
        if (passwordHash == default)
            throw new ArgumentException("Password hash can't be null", nameof(passwordHash));

        user.HashedPassword = passwordHasher.HashPassword(user, passwordHash);
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(User user, CancellationToken ct)
    {
        return Task.FromResult(user.HashedPassword)!;
    }

    public Task<bool> HasPasswordAsync(User user, CancellationToken ct)
    {
        return Task.FromResult(!string.IsNullOrEmpty(user.HashedPassword));
    }

    public Task SetEmailAsync(User user, string? email, CancellationToken ct)
    {
        if (email == default)
            throw new ArgumentException("Email can't be null!", nameof(email));

        user.Email = email;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(User user, CancellationToken ct)
    {
        return Task.FromResult(user.Email)!;
    }

    public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken ct)
    {
        return Task.FromResult(true);
    }

    public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken ct)
    {
        return Task.FromResult(true);
    }

    public async Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken ct)
    {
        return await userRepository.FindByNormalizedEmailOrDefaultAsync(normalizedEmail, ct);
    }

    public Task<string?> GetNormalizedEmailAsync(User user, CancellationToken ct)
    {
        return Task.FromResult(user.NormalizedEmail)!;
    }

    public Task SetNormalizedEmailAsync(User user, string? normalizedEmail, CancellationToken ct)
    {
        if (normalizedEmail == default)
            throw new ArgumentException("Email can't be null!", nameof(normalizedEmail));

        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }
}
