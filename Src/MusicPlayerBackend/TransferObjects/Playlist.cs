// ReSharper disable CheckNamespace
using System.ComponentModel.DataAnnotations;

namespace MusicPlayerBackend.TransferObjects.Playlist;

public enum TrackActionRequest
{
    Add, Delete
}

public enum TrackActionResponse
{
    Added, Deleted, AlreadyAdded, NotFound
}

public sealed class TrackRequestItem
{
    public required Guid Id { get; set; }
    public required TrackActionRequest Action { get; set; }
}

public sealed class TrackResponseItem
{
    public required Guid Id { get; set; }
    public required TrackActionResponse Action { get; set; }
}

public sealed class BulkTrackActionRequest
{
    public required TrackRequestItem[] Tracks { get; set; }
}

public sealed class BulkTrackActionResponse
{
    public required TrackResponseItem[] Tracks { get; set; }
}

public sealed class ClonePlaylistRequest
{
    [StringLength(maximumLength: 17, MinimumLength = 5, ErrorMessage = "Expected playlist name between 5 and 17")]
    public string? Name { get; set; }

    public bool IncludeTrackIds { get; set; }
}

public sealed class PlaylistResponse
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public string? CoverUri { get; set; }
    public Guid[]? TrackIds { get; set; }
}

public enum VisibilityLevel
{
    Private = 0,
    Public
}

public sealed class PlaylistListRequest : PaginationRequestBase;

public sealed class PlaylistListResponse
{
    [Required]
    public PlaylistListItemResponse[] Items { get; set; } = null!;

    public int TotalCount { get; set; }
}

public sealed class PlaylistListItemResponse
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public string? CoverUri { get; set; }
}

public sealed class CreatePlaylistRequest
{
    [Required]
    [StringLength(maximumLength: 17, MinimumLength = 5, ErrorMessage = "Expected playlist name between 5 and 17")]
    public string Name { get; set; } = null!;

    public VisibilityLevel? Visibility { get; set; } = VisibilityLevel.Private;
}

public sealed class UpdatePlaylistRequest
{
    public string? Name { get; set; }

    public VisibilityLevel? Visibility { get; set; }

    /// Removes existing cover. Provided cover with true value produce error.
    public bool RemoveCover { get; set; }

    public IFormFile? Cover { get; set; }
}

public sealed class UpdatePlaylistErrorResponse
{
    public string Error { get; set; } = null!;
}
