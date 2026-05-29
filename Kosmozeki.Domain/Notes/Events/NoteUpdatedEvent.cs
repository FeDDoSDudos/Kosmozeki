using Kosmozeki.Domain.Shared;

namespace Kosmozeki.Domain.Notes.Events;

public sealed class NoteUpdatedEvent : DomainEvent
{
    public NoteUpdatedEvent(Guid noteId, Guid roomId)
    {
        NoteId = noteId;
        RoomId = roomId;
    }

    public Guid NoteId { get; }
    public Guid RoomId { get; }
}