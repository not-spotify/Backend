namespace MusicPlayerBackend.Common;

public sealed class AppConfig
{
    public string JwtSecret { get; set; } = null!;
    public Minio Minio { get; set; } = null!;
    public bool MigrateDatabaseOnStartup { get; set; }
}

public sealed class Minio
{
    public string? Endpoint { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey  { get; set; }
}
