namespace MusicPlayerBackend.Persistence

open Microsoft.EntityFrameworkCore
open MusicPlayerBackend.Common
open MusicPlayerBackend.Persistence.Entities

[<Sealed>]
type FsharpAppDbContext(options) =
    inherit DbContext(options)

    static ConnectionStringName = "PgConnectionString"

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
