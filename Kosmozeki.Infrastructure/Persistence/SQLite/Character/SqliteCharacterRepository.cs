using Kosmozeki.Domain.Character;
using Microsoft.Data.Sqlite;
using System.Text.Json;
using CharacterDomain = Kosmozeki.Domain.Character.Character;

namespace Kosmozeki.Infrastructure.Persistence.SQLite.Character;

public sealed class SqliteCharacterRepository : ICharacterRepository
{
    private readonly SqliteConnection _connection;

    public SqliteCharacterRepository(SqliteConnection connection)
    {
        _connection = connection;
    }

    public async Task<CharacterDomain?> GetFirstAsync(CancellationToken ct = default)
    {
        var cmd = _connection.CreateCommand();
        cmd.CommandText = """
        SELECT Id, Name, BodyPartsJson
        FROM Characters
        ORDER BY Id
        LIMIT 1;
        """;

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;

        return new CharacterDomain
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            BodyParts = JsonSerializer.Deserialize<Dictionary<BodyPartType, BodyPart>>(reader.GetString(2))
                        ?? new()
        };
    }

    public async Task<CharacterDomain> UpsertAsync(CharacterDomain character, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(character.BodyParts);

        if (character.Id == 0)
        {
            var insert = _connection.CreateCommand();
            insert.CommandText = """
            INSERT INTO Characters (Name, BodyPartsJson)
            VALUES ($name, $bodyPartsJson);
            SELECT last_insert_rowid();
            """;
            insert.Parameters.AddWithValue("$name", character.Name);
            insert.Parameters.AddWithValue("$bodyPartsJson", json);

            var id = (long)(await insert.ExecuteScalarAsync(ct) ?? 0L);
            character.Id = (int)id;
            return character;
        }

        var update = _connection.CreateCommand();
        update.CommandText = """
        UPDATE Characters
        SET Name = $name,
            BodyPartsJson = $bodyPartsJson
        WHERE Id = $id;
        """;
        update.Parameters.AddWithValue("$id", character.Id);
        update.Parameters.AddWithValue("$name", character.Name);
        update.Parameters.AddWithValue("$bodyPartsJson", json);

        await update.ExecuteNonQueryAsync(ct);
        return character;
    }
}