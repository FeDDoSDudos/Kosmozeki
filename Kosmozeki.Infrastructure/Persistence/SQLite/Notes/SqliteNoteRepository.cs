using Kosmozeki.Domain.Notes;
using Kosmozeki.Domain.Shared;
using Kosmozeki.Infrastructure.Sqlite;
using Microsoft.Data.Sqlite;

namespace Kosmozeki.Infrastructure.Persistence.SQLite.Notes;

public sealed class SqliteNoteRepository : INoteRepository
{
    private readonly SqliteConnection _connection;
    private readonly SqliteUnitOfWork _unitOfWork;

    public SqliteNoteRepository(SqliteConnection connection, IUnitOfWork unitOfWork)
    {
        _connection = connection;
        _unitOfWork = (SqliteUnitOfWork)unitOfWork;
    }

    public async Task<SharedNote?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (_connection.State != System.Data.ConnectionState.Open)
            await _connection.OpenAsync(ct);

        await using var command = _connection.CreateCommand();
        command.Transaction = _unitOfWork.CurrentTransaction;
        command.CommandText = """
            select
                Id,
                RoomId,
                AuthorPlayerId,
                Content,
                Visibility,
                Version,
                UpdatedAt,
                IsDirty,
                IsDeleted,
                LastModifiedBy
            from Notes
            where Id = $id
            limit 1;
            """;

        command.Parameters.AddWithValue("$id", id.ToString());

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;

        return SqliteNoteMapper.Map(reader);
    }

    public async Task UpsertAsync(SharedNote note, CancellationToken ct = default) 
    {
        if (_connection.State != System.Data.ConnectionState.Open)
            await _connection.OpenAsync(ct);

        await using var command = _connection.CreateCommand();
        command.Transaction = _unitOfWork.CurrentTransaction;
        command.CommandText = """
            insert into Notes
            (
                Id,
                RoomId,
                AuthorPlayerId,
                Content,
                Visibility,
                Version,
                UpdatedAt,
                IsDirty,
                IsDeleted,
                LastModifiedBy
            )
            values
            (
                $id,
                $roomId,
                $authorPlayerId,
                $content,
                $visibility,
                $version,
                $updatedAt,
                $isDirty,
                $isDeleted,
                $lastModifiedBy
            )
            on conflict(Id) do update set
                RoomId = excluded.RoomId,
                AuthorPlayerId = excluded.AuthorPlayerId,
                Content = excluded.Content,
                Visibility = excluded.Visibility,
                Version = excluded.Version,
                UpdatedAt = excluded.UpdatedAt,
                IsDirty = excluded.IsDirty,
                IsDeleted = excluded.IsDeleted,
                LastModifiedBy = excluded.LastModifiedBy;
            """;

        command.Parameters.AddWithValue("$id", note.Id.ToString());
        command.Parameters.AddWithValue("$roomId", note.RoomId.ToString());
        command.Parameters.AddWithValue("$authorPlayerId", note.AuthorPlayerId.ToString());
        command.Parameters.AddWithValue("$content", note.Content);
        command.Parameters.AddWithValue("$visibility", note.Visibility.ToString());
        command.Parameters.AddWithValue("$version", note.Version.ToString("O"));
        command.Parameters.AddWithValue("$updatedAt", note.UpdatedAt.ToString("O"));
        command.Parameters.AddWithValue("$isDirty", note.IsDirty ? 1 : 0);
        command.Parameters.AddWithValue("$isDeleted", note.IsDeleted ? 1 : 0);
        command.Parameters.AddWithValue("$lastModifiedBy", (object?)note.LastModifiedBy ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(ct);
    }
}