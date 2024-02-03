using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Data.Repositories;

public interface ITrackPlaylistRepository : IEntityRepository<Guid, TrackPlaylist>
{
    Task AddTrackIfNotAdded(Guid playlistId, Guid trackId, CancellationToken ct = default);
    Task DeleteTrackIfNotAdded(Guid playlistId, Guid trackId, CancellationToken ct = default);
}

public sealed class TrackPlaylistRepository(AppDbContext dbContext) : EntityRepositoryBase<Guid, TrackPlaylist>(dbContext), ITrackPlaylistRepository
{
    public async Task AddTrackIfNotAdded(Guid playlistId, Guid trackId, CancellationToken ct = default)
    {
        var item = await QueryAll().Where(tp => tp.PlaylistId == playlistId && tp.TrackId == trackId).AnyAsync(ct);
        if (item != default)
            return;

        var newItem = new TrackPlaylist { PlaylistId = playlistId, TrackId = trackId };
        Save(newItem);
    }

    public async Task DeleteTrackIfNotAdded(Guid playlistId, Guid trackId, CancellationToken ct = default)
    {
        var item = await QueryMany(tp => tp.PlaylistId == playlistId && tp.TrackId == trackId).SingleOrDefaultAsync(ct);
        if (item == default)
            return;

        Delete(item);
    }
}
