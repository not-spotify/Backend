namespace MusicPlayerBackend.OptionSections

[<CLIMutable>]
type Minio = {
    Endpoint: string
    AccessKey: string
    SecretKey: string
    Port: int
    UseSsl: bool
}

[<CLIMutable>]
type TokenConfig = {
    SigningKey: string
    Issuer: string
    Audience: string
    ValidHours: float // 7 * 24
    RefreshValidHours: float // 7 * 24
}

[<CLIMutable>]
type AppConfig = {
    Minio: Minio
    MigrateDatabaseOnStartup: bool
}
