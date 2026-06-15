using Kosmozeki.Application.Common;
using Kosmozeki.Domain.Notes.Events;
using Kosmozeki.Domain.Shared;
using Microsoft.AspNetCore.SignalR;


namespace Kosmozeki.Infrastructure.Messaging;

public sealed class SignalREventDispatcher : IDomainEventDispatcher
{
    private readonly IRoomEventsPublisher _publisher;

    public SignalREventDispatcher(IRoomEventsPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task DispatchAsync(
        IEnumerable<DomainEvent> domainEvents,
        CancellationToken ct = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            switch (domainEvent)
            {
                case NoteCreatedEvent e:
                    await _publisher.PublishNotesChangedAsync(e.RoomId, e.NoteId, "note-created", ct);
                    break;

                case NoteUpdatedEvent e:
                    await _publisher.PublishNotesChangedAsync(e.RoomId, e.NoteId, "note-updated", ct);
                    break;

                case NoteDeletedEvent e:
                    await _publisher.PublishNotesChangedAsync(e.RoomId, e.NoteId, "note-deleted", ct);
                    break;
            }
        }
    }
}
