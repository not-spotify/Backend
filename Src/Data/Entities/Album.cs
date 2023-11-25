namespace MusicPlayerBackend.Data.Entities;

public record Album : EntityBase
{
    public string? CoverUrl { get; set; }
    public IEnumerable<AlbumTrack> AlbumTracks { get; set; } = null!;
}
