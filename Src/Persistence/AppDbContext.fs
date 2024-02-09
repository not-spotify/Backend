namespace MusicPlayerBackend.Persistence

open Microsoft.EntityFrameworkCore
open MusicPlayerBackend.Common
open MusicPlayerBackend.Persistence

[<Sealed>]
type FsharpAppDbContext(options) =
    inherit DbContext(options)

    static ConnectionStringName = "PgConnectionString"

    override _.OnModelCreating(modelBuilder) =
        %modelBuilder.Entity<Entities.User.User>()
             .HasIndex(indexExpression = fun s -> s.NormalizedUserName)
             .IsUnique()
        %modelBuilder.Entity<Entities.User.User>()
             .HasIndex(indexExpression = fun s -> s.NormalizedEmail)
             .IsUnique()
        %modelBuilder.Entity<Entities.User.User>(fun builder ->
            %builder
                .HasOne<Entities.Playlist.Playlist>()
                .WithMany()
                .HasForeignKey(foreignKeyExpression = fun c -> c.FavoritePlaylistId)
        )
