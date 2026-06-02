using Kosmozeki.Application.Common;
using Kosmozeki.Contracts.Notes.Dtos;
using Kosmozeki.Domain.Notes;
using Kosmozeki.Infrastructure.Persistence.Postgre;
using Microsoft.EntityFrameworkCore;

namespace Kosmozeki.Infrastructure.ReadDb;

public sealed class PostgresReadDb : IReadDb
{
    private readonly AppDbContext _dbContext;

    public PostgresReadDb(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<NoteDto>> QueryRoomNotesAsync(
        Guid roomId,
        bool @private,
        CancellationToken ct)
    {
        var query = _dbContext.Notes
            .AsNoTracking()
            .Where(x => x.RoomId == roomId && !x.IsDeleted);

        if (@private)
        {
            query = query.Where(x => x.Visibility == NoteVisibility.Private);
        }

        return await query
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new NoteDto(
                x.Id,
                x.RoomId,
                x.Content,
                x.AuthorPlayerId,
                null,
                null,
                x.Visibility.ToString(),
                x.UpdatedAt,
                x.IsDeleted))
            .ToListAsync(ct);
    }

    //public Task<IReadOnlyList<ItemDto>> QueryRoomInventoryAsync(Guid roomId, CancellationToken ct)
    //    => Task.FromResult<IReadOnlyList<ItemDto>>(Array.Empty<ItemDto>());

    //public Task<IReadOnlyList<ItemDto>> QueryPlayerInventoryAsync(Guid roomId, Guid playerId, CancellationToken ct)
    //    => Task.FromResult<IReadOnlyList<ItemDto>>(Array.Empty<ItemDto>());

    //public Task<IReadOnlyList<ItemTransferLogDto>> QueryItemHistoryAsync(Guid itemId, CancellationToken ct)
    //    => Task.FromResult<IReadOnlyList<ItemTransferLogDto>>(Array.Empty<ItemTransferLogDto>());

    //public Task<IReadOnlyList<PlayerDto>> QueryRoomPlayersAsync(Guid roomId, CancellationToken ct)
    //    => Task.FromResult<IReadOnlyList<PlayerDto>>(Array.Empty<PlayerDto>());
}