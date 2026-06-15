using Kosmozeki.Mobile.Options;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace Kosmozeki.Core.Realtime.Implementations;

public sealed class RoomRealtimeService : IRoomRealtimeService
{
    private readonly string _hubUrl;
    private HubConnection? _connection;
    private Guid? _joinedRoomId;

    public event Func<Guid, Task>? NotesChanged;

    public RoomRealtimeService(IOptions<ServerOptions> options)
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/')
            ?? throw new InvalidOperationException("ServerOptions.BaseUrl is not configured.");

        _hubUrl = $"{baseUrl}/hubs/room";
    }

    public async Task StartAsync(Guid roomId, CancellationToken ct = default)
    {
        if (_connection is null)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
                .WithAutomaticReconnect()
                .Build();

            _connection.On<NotesChangedMessage>("NotesChanged", async message =>
            {
                if (NotesChanged is not null)
                    await NotesChanged.Invoke(message.RoomId);
            });

            _connection.Reconnected += async _ =>
            {
                if (_connection is null || !_joinedRoomId.HasValue)
                    return;

                await _connection.InvokeAsync("JoinRoom", _joinedRoomId.Value.ToString(), CancellationToken.None);
            };
        }

        if (_connection.State == HubConnectionState.Disconnected)
        {
            await _connection.StartAsync(ct);
        }

        if (_connection.State != HubConnectionState.Connected)
            return;

        if (_joinedRoomId == roomId)
            return;

        if (_joinedRoomId.HasValue)
        {
            await _connection.InvokeAsync("LeaveRoom", _joinedRoomId.Value.ToString(), ct);
        }

        await _connection.InvokeAsync("JoinRoom", roomId.ToString(), ct);
        _joinedRoomId = roomId;
    }

    public async Task StopAsync(Guid roomId, CancellationToken ct = default)
    {
        if (_connection is null)
            return;

        if (_joinedRoomId == roomId && _connection.State == HubConnectionState.Connected)
        {
            await _connection.InvokeAsync("LeaveRoom", roomId.ToString(), ct);
            _joinedRoomId = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    private sealed record NotesChangedMessage(
        Guid RoomId,
        Guid NoteId,
        string Type,
        DateTimeOffset OccurredAt);
}