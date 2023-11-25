using Microsoft.Extensions.Logging;
using MusicPlayerBackend.Data.Entities;
using MusicPlayerBackend.Data.Repositories;
using Microsoft.AspNetCore.Identity;

namespace MusicPlayerBackend.Data.Identity.Stores;

public sealed class UserStore(ILogger<UserStore> logger, IUserRepository userRepository, ILookupNormalizer lookupNormalizer, IUnitOfWork unitOfWork) : IUserPasswordStore<User>, IUserEmailStore<User>
{
    public void Dispose()
    {
        logger.LogDebug("Called Dispose");
    }

    public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id.ToString());
    }

    public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email)!;
    }

    public async Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken)
    {
        if (userName == default)
            throw new ArgumentException("Can't set username to null", nameof(userName));

        user.Email = userName;
        userRepository.Save(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public async Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken)
    {
        if (normalizedName == default)
            throw new ArgumentException("Can't set username to null", nameof(normalizedName));

        user.NormalizedEmail = normalizedName;
        userRepository.Save(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
    {
        try
        {
            user.NormalizedUserName = lookupNormalizer.NormalizeName(user.UserName);
            user.NormalizedEmail = lookupNormalizer.NormalizeEmail(user.Email);

            userRepository.Save(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to create user: {user}", user);
            return IdentityResult.Failed();
        }
    }

    public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
    {
        try
        {
            user.NormalizedUserName = lookupNormalizer.NormalizeName(user.UserName);
            user.NormalizedEmail = lookupNormalizer.NormalizeEmail(user.Email);

            userRepository.Save(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to update user: {user}", user);
            return IdentityResult.Failed();
        }
    }

    public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
    {
        try
        {
            userRepository.Delete(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to delete user: {user}", user);
            return IdentityResult.Failed();
        }
    }

    public async Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        return await userRepository.GetByIdOrDefaultAsync(Guid.Parse(userId), cancellationToken);
    }

    public async Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        return await userRepository.FindByNormalizedEmailOrDefaultAsync(normalizedUserName, cancellationToken);
    }

    public async Task SetPasswordHashAsync(User user, string? passwordHash, CancellationToken cancellationToken)
    {
        if (passwordHash == default)
            throw new ArgumentException("Password hash can't be null", nameof(passwordHash));

        user.HashedPassword = passwordHash;
        userRepository.Save(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task<string?> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.HashedPassword)!;
    }

    public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    public async Task SetEmailAsync(User user, string? email, CancellationToken cancellationToken)
    {
        if (email == default)
            throw new ArgumentException("Email can't be null!", nameof(email));

        user.Email = email;
        userRepository.Save(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task<string?> GetEmailAsync(User user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email)!;
    }

    public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    public async Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        return await userRepository.FindByNormalizedEmailOrDefaultAsync(normalizedEmail, cancellationToken);
    }

    public Task<string?> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedEmail)!;
    }

    public async Task SetNormalizedEmailAsync(User user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        if (normalizedEmail == default)
            throw new ArgumentException("Email can't be null!", nameof(normalizedEmail));

        user.NormalizedEmail = normalizedEmail;
        userRepository.Save(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
