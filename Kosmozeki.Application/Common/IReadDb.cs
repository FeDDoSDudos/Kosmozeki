using Kosmozeki.Contracts.Notes.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Application.Common;

public interface IReadDb
{
    Task<IReadOnlyList<NoteDto>> QueryRoomNotesAsync(Guid roomId, bool @private, CancellationToken ct);
    //Task<IReadOnlyList<ItemDto>> QueryRoomInventoryAsync(Guid roomId, CancellationToken ct);
    //Task<IReadOnlyList<ItemDto>> QueryPlayerInventoryAsync(Guid roomId, Guid playerId, CancellationToken ct);
    //Task<IReadOnlyList<ItemTransferLogDto>> QueryItemHistoryAsync(Guid itemId, CancellationToken ct);
    //Task<IReadOnlyList<PlayerDto>> QueryRoomPlayersAsync(Guid roomId, CancellationToken ct);
}
