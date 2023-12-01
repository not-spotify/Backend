namespace MusicPlayerBackend.TransferObjects;

public sealed class LikedTrackListRequest : PaginationRequestBase;

public sealed class LikedTrackListResponse
{
    public IEnumerable<Guid> TrackIds { get; set; } = null!;
}
