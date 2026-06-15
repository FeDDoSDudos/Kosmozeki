using Kosmozeki.Contracts.Notes;
using Kosmozeki.Contracts.Notes.Dtos;
using Kosmozeki.Domain.Notes;
using Kosmozeki.Domain.Sync;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Kosmozeki.Mobile.Services;

public sealed class SyncBackgroundService : ISyncBackgroundService, IAsyncDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IRoomContext _roomContext;
    private readonly IPlayerIdentity _playerIdentity;
    private readonly ILogger<SyncBackgroundService> _logger;

    private readonly SemaphoreSlim _gate = new(1, 1);
    private PeriodicTimer? _timer;
    private CancellationTokenSource? _cts;
    private Task? _loopTask;

    public event Func<Task>? SyncCompleted;
    public event Func<string, Task>? SyncFailed;

    private readonly string _hubUrl;
    private HubConnection? _connection;
    private Guid? _joinedRoomId;
    public event Func<Guid, Task>? NotesChanged;
    private sealed record NotesChangedMessage(Guid RoomId, Guid NoteId, string Type, DateTimeOffset OccurredAt);

    public SyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        IRoomContext roomContext,
        IPlayerIdentity playerIdentity,
        ILogger<SyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _roomContext = roomContext;
        _logger = logger;
        _playerIdentity = playerIdentity;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        if (_loopTask is not null)
            return Task.CompletedTask;

        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
        _loopTask = RunLoopAsync(_cts.Token);

        Connectivity.ConnectivityChanged += OnConnectivityChanged;
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        Connectivity.ConnectivityChanged -= OnConnectivityChanged;

        if (_cts is null)
            return;

        _cts.Cancel();

        if (_loopTask is not null)
        {
            try
            {
                await _loopTask.WaitAsync(ct);
            }
            catch
            {
            }
        }

        _timer?.Dispose();
        _timer = null;
        _loopTask = null;
        _cts.Dispose();
        _cts = null;
    }

    public async Task TrySyncAsync(CancellationToken ct = default)
    {
        if (!await _gate.WaitAsync(0, ct))
            return;

        try
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                return;

            var roomId = _roomContext.CurrentRoomId;
            using var scope = _scopeFactory.CreateScope();

            var noteRepository = scope.ServiceProvider.GetRequiredService<INoteRepository>();
            var outbox = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var transport = scope.ServiceProvider.GetRequiredService<INotesSyncTransport>();

            await PushPendingAsync(outbox, transport, ct);
            await PullRemoteAsync(roomId, noteRepository, transport, ct);

            if (SyncCompleted is not null)
                await SyncCompleted.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync failed");

            if (SyncFailed is not null)
                await SyncFailed.Invoke(ex.Message);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task PushPendingAsync(
        IOutboxRepository outbox,
        INotesSyncTransport transport,
        CancellationToken ct)
    {
        var pending = await outbox.GetPendingAsync(100, ct);

        foreach (var entry in pending)
        {
            try
            {
                if (!string.Equals(entry.EntityType, nameof(SharedNote), StringComparison.Ordinal))
                    continue;

                var note = JsonSerializer.Deserialize<SharedNoteOutboxPayload>(entry.Payload);
                if (note is null)
                    continue;

                if (string.Equals(entry.Operation, "delete", StringComparison.OrdinalIgnoreCase))
                {
                    await transport.PushDeleteAsync(note.RoomId, note.Id, ct);
                }
                else
                {
                    var dto = new NoteDto(
                        note.Id,
                        note.RoomId,
                        note.Content,
                        note.AuthorPlayerId,
                        null,
                        null,
                        note.Visibility.ToString(),
                        note.UpdatedAt,
                        note.IsDeleted);

                    await transport.PushUpsertAsync(dto, ct);
                }

                await outbox.MarkProcessedAsync(entry.Id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to push outbox entry {EntryId}", entry.Id);
            }
        }
    }

    private async Task PullRemoteAsync(
        Guid roomId,
        INoteRepository noteRepository,
        INotesSyncTransport transport,
        CancellationToken ct)
    {
        var remoteNotes = await transport.PullRoomNotesAsync(roomId, _playerIdentity.PlayerId, includePrivate: false, ct);

        foreach (var dto in remoteNotes)
        {
            var local = await noteRepository.GetByIdAsync(dto.Id, ct);

            if (local is not null && local.IsDirty && local.UpdatedAt >= dto.UpdatedAt)
                continue;

            var synced = SharedNote.FromSync(
                dto.Id,
                dto.RoomId,
                dto.AuthorPlayerId,
                dto.Content,
                Enum.Parse<NoteVisibility>(dto.Visibility, true),
                dto.UpdatedAt,
                dto.IsDeleted);

            await noteRepository.UpsertAsync(synced, ct);
        }
    }

    private async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess == NetworkAccess.Internet)
            await TrySyncAsync();
    }

    private async Task RunLoopAsync(CancellationToken ct)
    {
        try
        {
            while (_timer is not null && await _timer.WaitForNextTickAsync(ct))
                await TrySyncAsync(ct);
        }
        catch (OperationCanceledException)
        {
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        _gate.Dispose();
    }
    private sealed record SharedNoteOutboxPayload(
        Guid Id,
        Guid RoomId,
        Guid AuthorPlayerId,
        string Content,
        NoteVisibility Visibility,
        DateTimeOffset Version,
        DateTimeOffset UpdatedAt,
        bool IsDirty,
        bool IsDeleted,
        string? LastModifiedBy);
}

