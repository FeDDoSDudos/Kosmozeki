using Kosmozeki.Application.Common;
using Kosmozeki.Contracts.Notes.Dtos;

namespace Kosmozeki.Application.Notes.GetRoomNotes;

public sealed class GetRoomNotesQueryHandler
    : IQueryHandler<GetRoomNotesQuery, IReadOnlyList<NoteDto>>
{
    private readonly IReadDb _readDb;
    private readonly ICache _cache;

    public GetRoomNotesQueryHandler(IReadDb readDb, ICache cache)
    {
        _readDb = readDb;
        _cache = cache;
    }

    public async Task<IReadOnlyList<NoteDto>> HandleAsync(
        GetRoomNotesQuery query,
        CancellationToken ct = default)
    {
        var cacheKey = NotesCacheKeys.Room(query.RoomId);

        var cached = await _cache.GetAsync<IReadOnlyList<NoteDto>>(cacheKey, ct);
        if (cached is not null)
            return cached;

        var notes = await _readDb.QueryRoomNotesAsync(query.RoomId, query.Private, ct);

        await _cache.SetAsync(cacheKey, notes, TimeSpan.FromMinutes(5), ct);

        return notes;
    }
}