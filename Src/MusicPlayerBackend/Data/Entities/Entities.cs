namespace MusicPlayerBackend.Data.Entities;

public record TrackPlaylist : EntityBase
{
    public Guid TrackId { get; set; }
    public Guid PlaylistId { get; set; }
    
    public virtual Track Track { get; set; }
    public virtual Playlist Playlist { get; set; }
}

public record Album : EntityBase
{
    public string? CoverUrl { get; set; }
    public IEnumerable<AlbumTrack> AlbumTracks { get; set; }
}

public sealed record Track : EntityBase
{
    public string? Cover { get; set; }
}

public record AlbumTrack : EntityBase
{
    public Guid AlbumId { get; set; }
    public Guid TrackId { get; set; }

    public virtual Album Album { get; set; }
    public virtual Track Track { get; set; }
}

public record Playlist : EntityBase
{
    public string Name { get; set; }
}
