namespace MusicPlayerBackend.TransferObjects;

public sealed class LikedTrackListRequest : PaginationRequestBase;

public sealed class LikedTrackListItem : Track.TrackListItem
{
    /// Track hidden or deleted. TrackUri will be empty if false.
    public bool IsAvailable { get; set; }

    private string? _trackUri;

    /// Empty if IsAvailable equal false
    public override string? TrackUri
    {
        get => IsAvailable ? _trackUri : null;
        set => _trackUri = value;
    }
}

public sealed class LikedTrackListResponse : ItemsResponseAbstract<LikedTrackListItem>;
