// ReSharper disable CheckNamespace
namespace MusicPlayerBackend.TransferObjects.User;

public sealed class RegisterRequest
{
    public string? UserName { get; set; }
    public string Email { get; set; }  = null!;
    public string Password { get; set; } = null!;
}

public sealed class RegisterResponse
{
    public Guid Id { get; set; }
}

public sealed class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public sealed class LoginResponse
{
    public Guid UserId { get; set; }
    public string JwtBearer { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTimeOffset RefreshTokenValidDue { get; set; }
    public DateTimeOffset JwtBearerValidDue { get; set; }
}

public sealed class UserResponse
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string Email { get; set; } = null!;
}

public sealed class RefreshRequest
{
    public Guid Jti { get; set; }
    public Guid RefreshToken { get; set; }
    public Guid UserId { get; set; }
}
