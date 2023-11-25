using Microsoft.AspNetCore.Identity;
using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Services.Identity;

public sealed class PasswordHasher : IPasswordHasher<User>
{
    public string HashPassword(User user, string password)
    {
        // YOLO
        return user.HashedPassword = password;
    }

    public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
    {
        return PasswordVerificationResult.Success;
    }
}
