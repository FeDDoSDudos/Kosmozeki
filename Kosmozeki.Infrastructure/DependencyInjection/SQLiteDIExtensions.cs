using Kosmozeki.Application.Common;
using Kosmozeki.Domain.Character;
using Kosmozeki.Domain.Notes;
using Kosmozeki.Domain.Shared;
using Kosmozeki.Domain.Sync;
using Kosmozeki.Domain.Weapon;
using Kosmozeki.Infrastructure.Persistence.SQLite.Character;
using Kosmozeki.Infrastructure.Persistence.SQLite.Notes;
using Kosmozeki.Infrastructure.Persistence.SQLite.Outbox;
using Kosmozeki.Infrastructure.Persistence.SQLite.Weapon;
using Kosmozeki.Infrastructure.ReadDb;
using Kosmozeki.Infrastructure.Sqlite;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace Kosmozeki.Infrastructure.DependencyInjection;

public static class SQLiteDIExtensions
{
    public static IServiceCollection AddKosmozekiMauiInfrastructure(
    this IServiceCollection services,
    string databasePath)
    {
        services.AddSingleton(_ =>
        {
            var connection = new SqliteConnection($"Data Source={databasePath}");
            SqliteDatabaseInitializer.InitializeAsync(connection).GetAwaiter().GetResult();
            return connection;
        });

        services.AddScoped<IUnitOfWork, SqliteUnitOfWork>();
        services.AddScoped<INoteRepository, SqliteNoteRepository>();
        services.AddScoped<IOutboxRepository, SqliteOutboxRepository>();
        services.AddScoped<ICharacterRepository, SqliteCharacterRepository>();
        services.AddScoped<IWeaponRepository, SqliteWeaponRepository>();
        services.AddSingleton<IReadDb, SqliteReadDb>();

        return services;
    }
}
