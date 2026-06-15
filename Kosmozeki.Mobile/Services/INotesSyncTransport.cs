using Kosmozeki.Contracts.Notes.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Mobile.Services;

public interface INotesSyncTransport
{
    Task PushUpsertAsync(NoteDto note, CancellationToken ct = default);
    Task PushDeleteAsync(Guid roomId, Guid noteId, CancellationToken ct = default);
    Task<IReadOnlyList<NoteDto>> PullRoomNotesAsync(Guid roomId, Guid playerId, bool includePrivate, CancellationToken ct = default);
}
