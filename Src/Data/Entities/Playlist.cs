using System;
using System.Collections.Generic;

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

    public Guid OwnerUserId { get; set; }
    public User OwnerUser { get; set; } = null!;

    public IEnumerable<PlaylistUserPermission> Permissions { get; set; } = null!;
}
