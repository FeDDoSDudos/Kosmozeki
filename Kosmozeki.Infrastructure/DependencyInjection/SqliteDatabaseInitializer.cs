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
        """;

        await cmd.ExecuteNonQueryAsync();
    }
}