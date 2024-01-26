namespace MusicPlayerBackend.Common;

public sealed class AppConfig
{
    public Minio Minio { get; set; } = null!;
    public bool MigrateDatabaseOnStartup { get; set; }
}

public sealed class Minio
{
    public string? Endpoint { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
    public int Port { get; set; }
}

public sealed class TokenConfig
{
    public string SigningKey { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
}
