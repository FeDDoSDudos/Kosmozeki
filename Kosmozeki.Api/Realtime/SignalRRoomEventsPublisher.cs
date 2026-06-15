using Kosmozeki.Api.Hubs;
using Kosmozeki.Application.Common;
using Microsoft.AspNetCore.SignalR;

namespace Kosmozeki.Api.Realtime;

public sealed class SignalRRoomEventsPublisher : IRoomEventsPublisher
{
    private readonly IHubContext<RoomHub> _hubContext;

    public SignalRRoomEventsPublisher(IHubContext<RoomHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task PublishNotesChangedAsync(
        Guid roomId,
        Guid noteId,
        string type,
        CancellationToken ct = default)
    {
        return _hubContext.Clients
            .Group($"room:{roomId}")
            .SendAsync("NotesChanged", new
            {
                RoomId = roomId,
                NoteId = noteId,
                Type = type,
                OccurredAt = DateTimeOffset.UtcNow
            }, ct);
    }
}
