using Microsoft.AspNetCore.Mvc;

// ReSharper disable once CheckNamespace
namespace MusicPlayerBackend.TransferObjects.Track;

public sealed class PlaylistListRequest : PaginationRequestBase;

public sealed class GetTracksInPlaylistRequest : PaginationRequestBase;

public enum TrackVisibility
{
    Hidden = 0,
    Visible = 1
}

public sealed class TrackUpdateRequest
{
    public TrackVisibility? Visibility { get; set; }

    /// Removes existing cover. Provided cover with true value produce error.
    public bool RemoveCover { get; set; }

    public IFormFile? Cover { get; set; }
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
    public Guid Id { get; set; }
    public string? CoverUri { get; set; }

    /// Track hidden or deleted. TrackUri will be empty if false.
    public bool IsAvailable { get; set; }

    private string? _trackUri;

    /// Empty if IsAvailable equal false
    public string? TrackUri
    {
        get => IsAvailable ? _trackUri : null;
        set => _trackUri = value;
    }

    public TrackVisibility Visibility { get; set; }

    public string Name { get; set; } = null!;
    public string Author { get; set; } = null!;
}

public sealed class TrackListResponse : ItemsResponseAbstract<TrackListItem>;

public class TrackResponse
{
    public Guid Id { get; set; }
    public string? CoverUri { get; set; }

    /// Track hidden or deleted. TrackUri will be empty if false.
    public bool IsAvailable { get; set; }

    private string? _trackUri;

    /// Empty if IsAvailable equal false
    public string? TrackUri
    {
        get => IsAvailable ? _trackUri : null;
        set => _trackUri = value;
    }

    public TrackVisibility Visibility { get; set; }

    public string Name { get; set; } = null!;
    public string Author { get; set; } = null!;
}

public sealed class UpdateTrackErrorResponse
{
    public string Error { get; set; } = null!;
}
