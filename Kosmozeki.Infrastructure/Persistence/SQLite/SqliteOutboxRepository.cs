using Kosmozeki.Domain.Shared;
using Kosmozeki.Domain.Sync;
using Microsoft.Data.Sqlite;

namespace Kosmozeki.Infrastructure.Sqlite;

public sealed class SqliteOutboxRepository : IOutboxRepository
{
    private readonly SqliteConnection _connection;
    private readonly SqliteUnitOfWork _unitOfWork;

    public SqliteOutboxRepository(SqliteConnection connection, IUnitOfWork unitOfWork)
    {
        _connection = connection;
        _unitOfWork = (SqliteUnitOfWork)unitOfWork;
    }

    public async Task AddAsync(OutboxEntry entry, CancellationToken ct = default)
    {
        if (_connection.State != System.Data.ConnectionState.Open)
            await _connection.OpenAsync(ct);

        await using var command = _connection.CreateCommand();
        command.Transaction = _unitOfWork.CurrentTransaction;
        command.CommandText = """
            insert into OutboxEntries
            (
                Id,
                EntityType,
                EntityId,
                Operation,
                Payload,
                RetryCount,
                CreatedAt,
                ProcessedAt
            )
            values
            (
                $id,
                $entityType,
                $entityId,
                $operation,
                $payload,
                $retryCount,
                $createdAt,
                $processedAt
            );
            """;

        command.Parameters.AddWithValue("$id", entry.Id.ToString());
        command.Parameters.AddWithValue("$entityType", entry.EntityType);
        command.Parameters.AddWithValue("$entityId", entry.EntityId.ToString());
        command.Parameters.AddWithValue("$operation", entry.Operation);
        command.Parameters.AddWithValue("$payload", entry.Payload);
        command.Parameters.AddWithValue("$retryCount", entry.RetryCount);
        command.Parameters.AddWithValue("$createdAt", entry.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("$processedAt", entry.ProcessedAt?.ToString("O") ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<IReadOnlyList<OutboxEntry>> GetPendingAsync(int take, CancellationToken ct = default)
    {
        if (_connection.State != System.Data.ConnectionState.Open)
            await _connection.OpenAsync(ct);

        var result = new List<OutboxEntry>();

        await using var command = _connection.CreateCommand();
        command.CommandText = """
            select
                Id,
                EntityType,
                EntityId,
                Operation,
                Payload,
                RetryCount,
                CreatedAt,
                ProcessedAt
            from OutboxEntries
            where ProcessedAt is null
            order by CreatedAt
            limit $take;
            """;

        command.Parameters.AddWithValue("$take", take);

        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(SqliteOutboxMapper.Map(reader));
        }

        return result;
    }

    public async Task MarkProcessedAsync(Guid id, CancellationToken ct = default)
    {
        if (_connection.State != System.Data.ConnectionState.Open)
            await _connection.OpenAsync(ct);

        await using var command = _connection.CreateCommand();
        command.Transaction = _unitOfWork.CurrentTransaction;
        command.CommandText = """
            update OutboxEntries
            set ProcessedAt = $processedAt
            where Id = $id;
            """;

        command.Parameters.AddWithValue("$id", id.ToString());
        command.Parameters.AddWithValue("$processedAt", DateTimeOffset.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync(ct);
    }
}