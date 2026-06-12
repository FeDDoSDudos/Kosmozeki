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

    public async Task<IReadOnlyList<NoteDto>> HandleAsync(
    GetRoomNotesQuery query,
    CancellationToken ct = default)
    {
        var notes = await _readDb.QueryRoomNotesAsync(query.RoomId, includePrivate: true, ct);

        var visible = notes
            .Where(x => !x.IsDeleted)
            .Where(x =>
                string.Equals(x.Visibility, "Public", StringComparison.OrdinalIgnoreCase) ||
                (string.Equals(x.Visibility, "Private", StringComparison.OrdinalIgnoreCase)
                 && x.AuthorPlayerId == query.CurrentPlayerId));

        if (query.IncludePrivateOnly)
        {
            visible = visible.Where(x =>
                string.Equals(x.Visibility, "Private", StringComparison.OrdinalIgnoreCase)
                && x.AuthorPlayerId == query.CurrentPlayerId);
        }

        return visible
            .OrderByDescending(x => x.UpdatedAt)
            .ToList();
    }
}