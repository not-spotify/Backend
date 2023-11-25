using Microsoft.EntityFrameworkCore;
using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Data;

public sealed class AppDbContext : DbContext
{
    public const string ConnectionStringName = "PgConnectionString";
    
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<Album> Albums { get; set; }
    public DbSet<Track> Tracks { get; set; }
    public DbSet<AlbumTrack> AlbumTracks { get; set; }
}