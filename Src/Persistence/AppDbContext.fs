namespace MusicPlayerBackend.Persistence

open Microsoft.EntityFrameworkCore
// open EntityFrameworkCore.FSharp.Extensions

open MusicPlayerBackend.Common
open MusicPlayerBackend.Persistence.Entities

[<Sealed>]
type FsharpAppDbContext(options) =
    inherit DbContext(options)

    static member ConnectionStringName = "PgConnectionString"

    [<DefaultValue>]
    val mutable playlists : DbSet<Playlist>
    member this.Playlists with get() = this.playlists and set v = this.playlists <- v


    [<DefaultValue>]
    val mutable playlistUserPermissions : DbSet<PlaylistUserPermission>
    member this.PlaylistUserPermissions with get() = this.playlistUserPermissions and set v = this.playlistUserPermissions <- v

    [<DefaultValue>]
    val mutable albums : DbSet<Album>
    member this.Albums with get() = this.albums and set v = this.albums <- v


    [<DefaultValue>]
    val mutable tracks : DbSet<Track>
    member this.Tracks with get() = this.tracks and set v = this.tracks <- v

    [<DefaultValue>]
    val mutable albumTracks : DbSet<AlbumTrack>
    member this.AlbumTracks with get() = this.albumTracks and set v = this.albumTracks <- v


    [<DefaultValue>]
    val mutable users : DbSet<User>
    member this.Users with get() = this.users and set v = this.users <- v

    [<DefaultValue>]
    val mutable refreshTokens : DbSet<RefreshToken>
    member this.RefreshTokens with get() = this.refreshTokens and set v = this.refreshTokens <- v


    override _.OnModelCreating(modelBuilder) =
        // %modelBuilder.RegisterSingleUnionCases()
        // %modelBuilder.RegisterOptionTypes()

        %modelBuilder.Entity<PlaylistUserPermission>()
            .HasKey("PlaylistId", "UserId", "Permission")

        %modelBuilder.Entity<TrackPlaylist>()
            .HasKey("TrackId", "PlaylistId")

        %modelBuilder.Entity<AlbumTrack>()
            .HasKey("AlbumId", "TrackId")

        %modelBuilder.Entity<User>()
             .HasIndex(indexExpression = fun s -> s.NormalizedUserName)
             .IsUnique()
        %modelBuilder.Entity<User>()
             .HasIndex(indexExpression = fun s -> s.NormalizedEmail)
             .IsUnique()
        %modelBuilder.Entity<User>(fun builder ->
            %builder
                .HasOne<Playlist>(fun p -> p.FavoritePlaylist).WithOne(fun p -> p.OwnerUser)
                .HasForeignKey<Playlist>(foreignKeyExpression = fun c -> c.OwnerUserId)
        )
