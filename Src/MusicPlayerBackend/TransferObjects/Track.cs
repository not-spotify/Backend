using System.ComponentModel.DataAnnotations;
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

public sealed class TrackListItem
{
    public string? CoverUri { get; set; }
    public string? TrackUri { get; set; }
    public TrackVisibility Visibility { get; set; }

    public string Name { get; set; } = null!;
    public string Author { get; set; } = null!;

    public Guid OwnerUserId { get; set; }
}

public sealed class TrackListResponse
{
    [Required]
    public TrackListItem[] Items { get; set; } = null!;

    public int TotalCount { get; set; }
}
