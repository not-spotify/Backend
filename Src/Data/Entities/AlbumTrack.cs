using System;

namespace MusicPlayerBackend.Data.Entities;

public record AlbumTrack : EntityBase
{
    public Guid AlbumId { get; set; }
    public Guid TrackId { get; set; }

    public Album Album { get; set; } = null!;
    public Track Track { get; set; } = null!;
}
