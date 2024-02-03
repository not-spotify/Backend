using System;

using Microsoft.EntityFrameworkCore;

namespace MusicPlayerBackend.Data.Entities;

public enum PlaylistPermission
{
    Full = 0,
    AllowedToModifyTracks = 1,
    AllowedToView = 2
}

[PrimaryKey(nameof(PlaylistId), nameof(UserId), nameof(Permission))]
public sealed record PlaylistUserPermission : EntityBase
{
    public Guid PlaylistId { get; set; }
    public Guid UserId { get; set; }

    public PlaylistPermission Permission { get; set; }

    public Playlist Playlist { get; set; } = null!;
    public User User { get; set; } = null!;
}
