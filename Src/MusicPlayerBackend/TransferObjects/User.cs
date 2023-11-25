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
    public string JwtBearer { get; set; } = null!;
}
