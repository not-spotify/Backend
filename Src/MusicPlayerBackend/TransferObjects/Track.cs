using Microsoft.AspNetCore.Mvc;

// ReSharper disable once CheckNamespace
namespace MusicPlayerBackend.TransferObjects.Track;

public sealed class PlaylistListRequest : PaginationRequestBase;

public enum TrackVisibility
{
    Hidden = 0,
    Visible = 1
}

public sealed class TrackUpdateRequest
{
    public TrackVisibility Visibility { get; set; }
    public string? CoverUri { get; set; }
    public string? Name { get; set; }
}

public sealed class TrackCreateRequest
{
    public string Name { get; set; } = null!;
    public string Author { get; set; } = null!;

    public IFormFile Track { get; set; } = null!;

    [FromForm]
    public IFormFile? Cover { get; set; }

    public TrackVisibility Visibility { get; set; }
}

public class TrackListItem
{
    public string? CoverUri { get; set; }
    public virtual string? TrackUri { get; set; }
    public TrackVisibility Visibility { get; set; }

    public string Name { get; set; } = null!;
    public string Author { get; set; } = null!;

    public Guid OwnerUserId { get; set; }
}

public sealed class TrackListResponse : ItemsResponseAbstract<TrackListItem>;
