using System.Reflection;
using Kosmozeki.Domain.Notes;

namespace Kosmozeki.Infrastructure.Sqlite;

internal static class SqliteNoteMapper
{
    public static SharedNote Map(Microsoft.Data.Sqlite.SqliteDataReader reader)
    {
        var note = (SharedNote)Activator.CreateInstance(typeof(SharedNote), nonPublic: true)!;

        Set(note, "Id", Guid.Parse(reader.GetString(0)));
        Set(note, "RoomId", Guid.Parse(reader.GetString(1)));
        Set(note, "AuthorPlayerId", Guid.Parse(reader.GetString(2)));
        Set(note, "Content", reader.GetString(3));
        Set(note, "Visibility", Enum.Parse(typeof(NoteVisibility), reader.GetString(4)));
        Set(note, "Version", reader.GetInt64(5));
        Set(note, "UpdatedAt", DateTimeOffset.Parse(reader.GetString(6)));
        Set(note, "IsDirty", reader.GetInt64(7) == 1);
        Set(note, "IsDeleted", reader.GetInt64(8) == 1);
        Set(note, "LastModifiedBy", reader.IsDBNull(9) ? null : reader.GetString(9));

        return note;
    }

    private static void Set(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        property!.SetValue(target, value);
    }
}