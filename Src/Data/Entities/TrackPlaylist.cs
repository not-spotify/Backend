namespace MusicPlayerBackend.Data.Entities;

public record TrackPlaylist : EntityBase
{
    public Guid TrackId { get; set; }
    public Guid PlaylistId { get; set; }

    public Track Track { get; set; } = null!;
    public Playlist Playlist { get; set; } = null!;
}
