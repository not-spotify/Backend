namespace MusicPlayerBackend.Data.Entities;

public sealed record User : EntityBase
{
    public string? UserName { get; set; }
    public string? NormalizedUserName { get; set; }
    public string Email { get; set; } = null!;
    public string NormalizedEmail { get; set; } = null!;
    public string HashedPassword { get; set; } = null!;
    public Guid FavoriteTracksPlaylistId { get; set; }

    public Playlist LickedTracksPlaylist { get; set; } = null!;
    public IEnumerable<PlaylistUserPermission> Permissions { get; set; } = null!;
}
