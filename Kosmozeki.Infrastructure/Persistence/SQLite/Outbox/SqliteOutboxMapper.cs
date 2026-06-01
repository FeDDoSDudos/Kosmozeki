using System.Reflection;
using Kosmozeki.Domain.Shared;

namespace Kosmozeki.Infrastructure.Persistence.SQLite.Outbox;

internal static class SqliteOutboxMapper
{
    public static OutboxEntry Map(Microsoft.Data.Sqlite.SqliteDataReader reader)
    {
        var entry = (OutboxEntry)Activator.CreateInstance(typeof(OutboxEntry), nonPublic: true)!;

        Set(entry, "Id", Guid.Parse(reader.GetString(0)));
        Set(entry, "EntityType", reader.GetString(1));
        Set(entry, "EntityId", Guid.Parse(reader.GetString(2)));
        Set(entry, "Operation", reader.GetString(3));
        Set(entry, "Payload", reader.GetString(4));
        Set(entry, "RetryCount", reader.GetInt32(5));
        Set(entry, "CreatedAt", DateTimeOffset.Parse(reader.GetString(6)));
        Set(entry, "ProcessedAt", reader.IsDBNull(7) ? null : DateTimeOffset.Parse(reader.GetString(7)));

        return entry;
    }

    private static void Set(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        property!.SetValue(target, value);
    }
}