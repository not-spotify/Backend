namespace MusicPlayerBackend.Persistence

open Microsoft.EntityFrameworkCore
open MusicPlayerBackend.Common
open MusicPlayerBackend.Persistence.Entities

[<Sealed>]
type FsharpAppDbContext(options) =
    inherit DbContext(options)

    static ConnectionStringName = "PgConnectionString"

    member val Playlists =
        Playlist
            .Default with get, set

    member val PlaylistUserPermissions =
        PlaylistUserPermission
            .Default with get, set

    member val Albums =
        Album
            .Default with get, set

    member val Tracks =
        Track
            .Default with get, set

    member val AlbumTracks =
        AlbumTrack
            .Default with get, set

    member val Users =
        User
            .Default with get, set

    member val RefreshTokens =
        RefreshToken
            .Default with get, set

    override _.OnModelCreating(modelBuilder) =
        %modelBuilder.Entity<User>()
             .HasIndex(indexExpression = fun s -> s.NormalizedUserName)
             .IsUnique()
        %modelBuilder.Entity<User>()
             .HasIndex(indexExpression = fun s -> s.NormalizedEmail)
             .IsUnique()
        %modelBuilder.Entity<User>(fun builder ->
            %builder
                .HasOne<Playlist>()
                .WithMany()
                .HasForeignKey(foreignKeyExpression = fun c -> c.FavoritePlaylistId)
        )
