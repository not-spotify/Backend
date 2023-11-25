namespace MusicPlayerBackend.Data.Entities;

public record TrackPlaylist : EntityBase
{
    public Guid TrackId { get; set; }
    public Guid PlaylistId { get; set; }
    
    public Track Track { get; set; } = null!;
    public Playlist Playlist { get; set; } = null!;
}

public record Album : EntityBase
{
    public string? CoverUrl { get; set; }
    public IEnumerable<AlbumTrack> AlbumTracks { get; set; } = null!;
}

public sealed record Track : EntityBase
{
    public string? Cover { get; set; }
}

public record AlbumTrack : EntityBase
{
    public Guid AlbumId { get; set; }
    public Guid TrackId { get; set; }

    public Album Album { get; set; } = null!;
    public Track Track { get; set; } = null!;
}

public enum PlaylistVisibility
{
    Private = 0, 
    Public
}

public record Playlist : EntityBase
{
    public string Name { get; set; } = null!;
    public PlaylistVisibility Visibility { get; set; }
    public string? CoverUri { get; set; }
}
