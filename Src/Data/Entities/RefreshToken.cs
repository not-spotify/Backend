namespace MusicPlayerBackend.Data.Entities;

public sealed record RefreshToken : EntityBase
{
    public Guid UserId { get; set; }
    public Guid Jti { get; set; }
    public Guid Token { get; set; }
    public DateTimeOffset ValidDue { get; set; }
    public bool Revoked { get; set; }

    public User User { get; set; } = null!;
}
