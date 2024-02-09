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
}

[<CLIMutable>]
type AppConfig = {
    Minio: Minio
    MigrateDatabaseOnStartup: bool
}
