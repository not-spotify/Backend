namespace MusicPlayerBackend.Data.Entities;

public enum TrackVisibility
{
    Hidden = 0,
    Visible = 1
}

public sealed record Track : EntityBase
{
    public string? CoverUri { get; set; }
    public string? TrackUri { get; set; }
    public TrackVisibility Visibility { get; set; }

    public string Name { get; set; } = null!;
    public string Author { get; set; } = null!;

    public Guid OwnerUserId { get; set; }
    public User OwnerUser { get; set; } = null!;
}
