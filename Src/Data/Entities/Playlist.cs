namespace MusicPlayerBackend.Data.Entities;

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
