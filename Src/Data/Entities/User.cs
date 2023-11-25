namespace MusicPlayerBackend.Data.Entities;

public sealed record User : EntityBase
{
    public string? UserName { get; set; }
    public string? NormalizedUserName { get; set; }

    public IEnumerable<PlaylistUserPermission> Permissions { get; set; } = null!;
}
