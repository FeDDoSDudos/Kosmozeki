using Microsoft.Data.Sqlite;

namespace Kosmozeki.Infrastructure.Sqlite;

public static class SqliteDatabaseInitializer
{
    public static async Task InitializeAsync(SqliteConnection connection)
    {
        await connection.OpenAsync();

        var cmd = connection.CreateCommand();
        cmd.CommandText = """
        CREATE TABLE IF NOT EXISTS Characters (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            BodyPartsJson TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS Weapons (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            BaseDMG INTEGER NOT NULL,
            EDMG INTEGER NOT NULL,
            MaxAmmo INTEGER NOT NULL,
            CurrentAmmo INTEGER NOT NULL,
            ROF INTEGER NOT NULL,
            BaseDIF INTEGER NOT NULL
        );

        CREATE TABLE IF NOT EXISTS Notes (
            Id TEXT PRIMARY KEY,
            RoomId TEXT NOT NULL,
            AuthorPlayerId TEXT NOT NULL,
            Content TEXT NOT NULL,
            Visibility TEXT NOT NULL,
            Version TEXT NOT NULL,
            UpdatedAt TEXT NOT NULL,
            IsDirty INTEGER NOT NULL,
            IsDeleted INTEGER NOT NULL,
            LastModifiedBy TEXT NULL
        );

        CREATE TABLE IF NOT EXISTS OutboxEntries (
            Id TEXT PRIMARY KEY,
            EntityType TEXT NOT NULL,
            EntityId TEXT NOT NULL,
            Operation TEXT NOT NULL,
            Payload TEXT NOT NULL,
            RetryCount INTEGER NOT NULL,
            CreatedAt TEXT NOT NULL,
            ProcessedAt TEXT NULL
        );

        CREATE INDEX IF NOT EXISTS IX_Notes_RoomId ON Notes(RoomId);
        CREATE INDEX IF NOT EXISTS IX_Notes_RoomId_IsDeleted ON Notes(RoomId, IsDeleted);
        CREATE INDEX IF NOT EXISTS IX_OutboxEntries_ProcessedAt ON OutboxEntries(ProcessedAt);
        CREATE INDEX IF NOT EXISTS IX_OutboxEntries_EntityId ON OutboxEntries(EntityId);
        """;

        await cmd.ExecuteNonQueryAsync();
    }
}