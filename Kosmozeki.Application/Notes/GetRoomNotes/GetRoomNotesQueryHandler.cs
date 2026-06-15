using Kosmozeki.Application.Common;
using Kosmozeki.Contracts.Notes.Dtos;

namespace Kosmozeki.Application.Notes.GetRoomNotes;

public sealed class GetRoomNotesQueryHandler
    : IQueryHandler<GetRoomNotesQuery, IReadOnlyList<NoteDto>>
{
    private readonly IReadDb _readDb;

    public GetRoomNotesQueryHandler(IReadDb readDb)
    {
        _readDb = readDb;
    }

    public Task<IReadOnlyList<NoteDto>> HandleAsync(
    GetRoomNotesQuery query,
    CancellationToken ct = default)
    {
        return _readDb.QueryRoomNotesAsync(
            query.RoomId,
            query.CurrentPlayerId,
            query.IncludePrivate,
            ct);
    }
}