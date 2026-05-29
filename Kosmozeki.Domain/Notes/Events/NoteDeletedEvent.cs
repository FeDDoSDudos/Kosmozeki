using Kosmozeki.Domain.Shared;

namespace Kosmozeki.Domain.Notes.Events;

public sealed class NoteDeletedEvent : DomainEvent
{
    public NoteDeletedEvent(Guid noteId, Guid roomId)
    {
        NoteId = noteId;
        RoomId = roomId;
    }

    public Guid NoteId { get; }
    public Guid RoomId { get; }
}
