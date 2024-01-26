namespace MusicPlayerBackend.TransferObjects;

public sealed class FavoriteTrackListRequest : PaginationRequestBase;

public sealed class FavoriteTrackListItem : Track.TrackListItem;

public sealed class FavoriteTrackListResponse : ItemsResponseAbstract<FavoriteTrackListItem>;
