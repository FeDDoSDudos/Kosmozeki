using Kosmozeki.Domain.Notes.Events;
using Kosmozeki.Domain.Shared;

namespace Kosmozeki.Domain.Notes;

public sealed class SharedNote : SyncableEntity
{
    private SharedNote()
    {
    }

    public string Content { get; private set; } = string.Empty;
    public Guid AuthorPlayerId { get; private set; }
    public NoteVisibility Visibility { get; private set; }

    public static SharedNote Create(
        Guid roomId,
        Guid authorPlayerId,
        string content,
        NoteVisibility visibility,
        string? lastModifiedBy = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        var now = DateTimeOffset.UtcNow;

        var note = new SharedNote
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            AuthorPlayerId = authorPlayerId,
            Content = content.Trim(),
            Visibility = visibility,
            Version = now,
            UpdatedAt = now,
            IsDirty = true,
            IsDeleted = false,
            LastModifiedBy = lastModifiedBy
        };

        note.RaiseDomainEvent(new NoteCreatedEvent(note.Id, note.RoomId));
        return note;
    }

    public static SharedNote FromSync(
        Guid id,
        Guid roomId,
        Guid authorPlayerId,
        string content,
        NoteVisibility visibility,
        DateTimeOffset updatedAt,
        bool isDeleted,
        string? lastModifiedBy = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        return new SharedNote
        {
            Id = id,
            RoomId = roomId,
            AuthorPlayerId = authorPlayerId,
            Content = content.Trim(),
            Visibility = visibility,
            Version = updatedAt,
            UpdatedAt = updatedAt,
            IsDirty = false,
            IsDeleted = isDeleted,
            LastModifiedBy = lastModifiedBy
        };
    }

    public void Update(
        string content,
        NoteVisibility visibility,
        string? lastModifiedBy = null)
    {
        EnsureNotDeleted();
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        Content = content.Trim();
        Visibility = visibility;
        LastModifiedBy = lastModifiedBy;

        Touch();

        RaiseDomainEvent(new NoteUpdatedEvent(Id, RoomId));
    }

    public void Delete(string? lastModifiedBy = null)
    {
        EnsureNotDeleted();

        IsDeleted = true;
        LastModifiedBy = lastModifiedBy;
        Touch();

        RaiseDomainEvent(new NoteDeletedEvent(Id, RoomId));
    }

    private void Touch()
    {
        var now = DateTimeOffset.UtcNow;
        UpdatedAt = now;
        Version = now;
        IsDirty = true;
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Note is deleted.");
    }
}