using Microsoft.Data.Sqlite;

namespace Kosmozeki.Infrastructure.Sqlite;

public static class SqliteDatabaseInitializer
{
    public static async Task InitializeAsync(SqliteConnection connection, CancellationToken ct = default)
    {
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            create table if not exists Notes
            (
                Id text primary key,
                RoomId text not null,
                AuthorPlayerId text not null,
                Content text not null,
                Visibility text not null,
                Version integer not null,
                UpdatedAt text not null,
                IsDirty integer not null,
                IsDeleted integer not null,
                LastModifiedBy text null
            );

            create table if not exists OutboxEntries
            (
                Id text primary key,
                EntityType text not null,
                EntityId text not null,
                Operation text not null,
                Payload text not null,
                RetryCount integer not null,
                CreatedAt text not null,
                ProcessedAt text null
            );

            create index if not exists IX_Notes_RoomId on Notes(RoomId);
            create index if not exists IX_Notes_RoomId_UpdatedAt on Notes(RoomId, UpdatedAt);
            create index if not exists IX_OutboxEntries_ProcessedAt on OutboxEntries(ProcessedAt);
            create index if not exists IX_OutboxEntries_CreatedAt on OutboxEntries(CreatedAt);
            """;

        await command.ExecuteNonQueryAsync(ct);
    }
}