using Microsoft.EntityFrameworkCore;
using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Data;

public sealed class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public const string ConnectionStringName = "PgConnectionString";

    public DbSet<Playlist> Playlists { get; set; } = null!;
    public DbSet<PlaylistUserPermission> PlaylistUserPermissions { get; set; } = null!;
    public DbSet<Album> Albums { get; set; } = null!;
    public DbSet<Track> Tracks { get; set; } = null!;
    public DbSet<AlbumTrack> AlbumTracks { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<LikedTrack> LikedTracks { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
}
