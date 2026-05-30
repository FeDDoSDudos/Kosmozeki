using Kosmozeki.Application.Common;
using Kosmozeki.Contracts.Notes.Dtos;
using Microsoft.Data.Sqlite;

namespace Kosmozeki.Infrastructure.ReadDb;

public sealed class SqliteReadDb : IReadDb
{
    private readonly SqliteConnection _connection;

    public SqliteReadDb(SqliteConnection connection)
    {
        _connection = connection;
    }

    public async Task<IReadOnlyList<NoteDto>> QueryRoomNotesAsync(
        Guid roomId,
        bool masterOnly,
        CancellationToken ct)
    {
        if (_connection.State != System.Data.ConnectionState.Open)
            await _connection.OpenAsync(ct);

        var result = new List<NoteDto>();

        await using var command = _connection.CreateCommand();
        command.CommandText = """
            select
                Id,
                RoomId,
                Content,
                AuthorPlayerId,
                Visibility,
                UpdatedAt,
                IsDeleted
            from Notes
            where RoomId = $roomId
              and IsDeleted = 0
              and ($masterOnly = 0 or Visibility = 'MasterOnly')
            order by UpdatedAt desc;
            """;

        command.Parameters.AddWithValue("$roomId", roomId.ToString());
        command.Parameters.AddWithValue("$masterOnly", masterOnly ? 1 : 0);

        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(new NoteDto(
                Guid.Parse(reader.GetString(0)),
                Guid.Parse(reader.GetString(1)),
                reader.GetString(2),
                Guid.Parse(reader.GetString(3)),
                null,
                null,
                reader.GetString(4),
                DateTimeOffset.Parse(reader.GetString(5)),
                reader.GetInt64(6) == 1));
        }

        return result;
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