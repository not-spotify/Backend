namespace MusicPlayerBackend.Data.Entities;

public sealed record LikedTrack : EntityBase
{
    public Guid UserId { get; set; }
    public Guid TrackId { get; set; }

    public User User { get; set; } = null!;
    public Track Track { get; set; } = null!;
}
