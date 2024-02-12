﻿using System.ComponentModel.DataAnnotations;
using MusicPlayerBackend.TransferObjects.Track;

// ReSharper disable CheckNamespace
namespace MusicPlayerBackend.TransferObjects.Playlist;

public enum TrackActionRequest
{
    Add = 0,
    Delete = 1
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

public enum VisibilityLevel
{
    Private,
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
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? CoverUri { get; set; }
    public VisibilityLevel Visibility { get; set; }
}

public sealed class UpdatePlaylistErrorResponse
{
    public UpdatePlaylistErrorResponse() {}

    public UpdatePlaylistErrorResponse(string error)
    {
        Error = error;
    }
    public string Error { get; set; } = null!;
}

public sealed class TrackInPlaylistListItem : TrackListItem;

public sealed class TrackInPlaylistListResponse : ItemsResponseAbstract<TrackInPlaylistListItem>;
