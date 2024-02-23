namespace MusicPlayerBackend.Persistence.Entities

open System
open System.ComponentModel.DataAnnotations
open System.Linq

type UserId = Guid
type PlaylistId = Guid
type AlbumId = Guid
type TrackId = Guid
type RefreshTokenId = Guid

type PlaylistPermission =
    | Full = 0
    | ModifyTrack = 1
    | View = 2

type [<CLIMutable>] PlaylistUserPermission = {
    mutable PlaylistId: PlaylistId
    mutable UserId: UserId

    Playlist: Playlist
    User: User
    Permission: PlaylistPermission

    mutable CreatedAt: DateTimeOffset
    mutable UpdatedAt: DateTimeOffset option
}

and [<CLIMutable>] User = {
    mutable Id: UserId

    mutable UserName: string
    mutable NormalizedUserName: string
    mutable Email: string
    mutable NormalizedEmail: string
    mutable HashedPassword: string
    mutable FavoritePlaylistId: PlaylistId

    FavoritePlaylist: Playlist
    Permissions: IQueryable<PlaylistUserPermission>

    mutable CreatedAt: DateTimeOffset
    mutable UpdatedAt: DateTimeOffset option
}
with
    static member Create(userName, email, hashedPassword, favoritePlaylistId) : User = {
        Id = Guid.Empty
        UserName = userName
        NormalizedUserName = null
        Email = email
        NormalizedEmail = null
        HashedPassword = hashedPassword
        FavoritePlaylistId = favoritePlaylistId
        FavoritePlaylist = Unchecked.defaultof<_>
        Permissions = null
        CreatedAt = DateTimeOffset.MinValue
        UpdatedAt = None
    }

and Visibility =
    | Private = 0
    | Public = 1

and [<CLIMutable>] Playlist = {
    mutable Id: PlaylistId

    mutable Name: string
    mutable Visibility: Visibility
    mutable CoverUri: string option
    mutable OwnerUserId: UserId

    OwnerUser: User
    TrackPlaylists: IQueryable<TrackPlaylist>
    Permissions: IQueryable<PlaylistUserPermission>

    mutable CreatedAt: DateTimeOffset
    mutable UpdatedAt: DateTimeOffset option
}
with
    static member Create(name, visibility, coverUri, ownerUserId) = {
        Id = Guid.Empty
        Name = name
        Visibility = visibility
        CoverUri = coverUri
        OwnerUserId = ownerUserId
        OwnerUser = Unchecked.defaultof<_>
        TrackPlaylists = null
        Permissions = null
        CreatedAt = DateTimeOffset.MinValue
        UpdatedAt = None
    }

and [<CLIMutable>] Album = {
    mutable Id: AlbumId

    mutable CoverUri: string option

    AlbumTracks: IQueryable<AlbumTrack>

    mutable CreatedAt: DateTimeOffset
    mutable UpdatedAt: DateTimeOffset option
}

and Jti = Guid

and [<CLIMutable>] RefreshToken = {
    mutable Id: RefreshTokenId
    mutable Jti: Jti
    mutable Token: Guid
    mutable Revoked: bool
    mutable UserId: UserId

    mutable User: User

    mutable ValidDue: DateTimeOffset
    mutable CreatedAt: DateTimeOffset
    mutable UpdatedAt: DateTimeOffset option
}
with
    static member Create(userId, validDue, jti, token) = {
        Id = Guid.Empty
        ValidDue = validDue
        Jti = jti
        Token = token
        Revoked = false
        CreatedAt = DateTimeOffset.MinValue
        UpdatedAt = None
        UserId = userId
        User = Unchecked.defaultof<_>
    }

and TrackVisibility =
    | Hidden = 0
    | Visible = 1

and [<CLIMutable>] Track = {
    [<Key>]
    mutable Id: TrackId
    mutable OwnerUserId: UserId
    mutable OwnerUser: User

    mutable CoverUri: string option
    mutable TrackUri: string
    mutable Visibility: TrackVisibility

    mutable Name: string
    mutable Author: string

    mutable CreatedAt: DateTimeOffset
    mutable UpdatedAt: DateTimeOffset option
}
with
    static member Create(ownerUserId, coverUri, trackUri, visibility, name, author) = {
        Id = Guid.Empty
        OwnerUserId = ownerUserId
        OwnerUser = Unchecked.defaultof<_>
        CoverUri = coverUri
        TrackUri = trackUri
        Visibility = visibility
        Name = name
        Author = author
        CreatedAt = DateTimeOffset.MinValue
        UpdatedAt = None
    }

and [<CLIMutable>] TrackPlaylist = {
    mutable TrackId: TrackId
    mutable PlaylistId: PlaylistId

    Track: Track
    Playlist: Playlist

    mutable AddedAt: DateTimeOffset
}
with
    static member Create(trackId, playlistId) = {
        TrackId = trackId
        PlaylistId = playlistId
        Track = Unchecked.defaultof<_>
        Playlist = Unchecked.defaultof<_>
        AddedAt = DateTimeOffset.MinValue

    }

and [<CLIMutable>] AlbumTrack = {
    mutable AlbumId: AlbumId
    mutable TrackId: TrackId

    Album: Album
    Track: Track

    mutable CreatedAt: DateTimeOffset
}
