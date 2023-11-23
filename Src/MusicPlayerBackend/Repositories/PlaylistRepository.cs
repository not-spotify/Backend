﻿using MusicPlayerBackend.Data;
using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Repositories;

public interface IPlaylistRepository : IEntityRepository<Guid, Playlist>
{
}

public sealed class PlaylistRepository(AppDbContext dbContext) : EntityRepositoryBase<Guid, Playlist>(dbContext), IPlaylistRepository;