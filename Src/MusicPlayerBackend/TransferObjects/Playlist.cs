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
    public required IEnumerable<TrackRequestItem> Tracks { get; set; }
}

public sealed class BulkTrackActionResponse
{
    public required IEnumerable<TrackResponseItem> Tracks { get; set; }
}

public sealed class ClonePlaylistRequest
{
    public string? Name { get; set; }
}

public sealed class PlaylistResponse
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required IEnumerable<Guid> TrackIds { get; set; }
}