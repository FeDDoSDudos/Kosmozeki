# Exported project files

Root: C:\Users\frolo\source\repos\Kosmozeki
Generated: 2026-06-12 22:21:50

---

## FILE: Kosmozeki.Application/Notes/GetRoomNotes/GetRoomNotesQuery.cs

```cs
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Application.Notes.GetRoomNotes;

public sealed record GetRoomNotesQuery(
    Guid RoomId,
    Guid CurrentPlayerId,
    bool IncludePrivateOnly = false);

```

---

## FILE: Kosmozeki.Application/Notes/GetRoomNotes/GetRoomNotesQueryHandler.cs

```cs
using Kosmozeki.Application.Common;
using Kosmozeki.Contracts.Notes.Dtos;

namespace Kosmozeki.Application.Notes.GetRoomNotes;

public sealed class GetRoomNotesQueryHandler
    : IQueryHandler<GetRoomNotesQuery, IReadOnlyList<NoteDto>>
{
    private readonly IReadDb _readDb;

    public GetRoomNotesQueryHandler(IReadDb readDb)
    {
        _readDb = readDb;
    }

    public Task<IReadOnlyList<NoteDto>> HandleAsync(
        GetRoomNotesQuery query,
        CancellationToken ct = default)
    {
        return _readDb.QueryRoomNotesAsync(query.RoomId, query.IncludePrivateOnly, ct);
    }
}
```

---

## FILE: Kosmozeki.Api/Controllers/NotesController.cs

```cs
using Kosmozeki.Api.Contracts.Notes;
using Kosmozeki.Application.Common;
using Kosmozeki.Application.Notes.CreateNote;
using Kosmozeki.Application.Notes.DeleteNote;
using Kosmozeki.Application.Notes.GetRoomNotes;
using Kosmozeki.Application.Notes.UpdateNote;
using Kosmozeki.Contracts.Notes.Dtos;
using Kosmozeki.Domain.Notes;
using Microsoft.AspNetCore.Mvc;

namespace Kosmozeki.Server.Controllers;

[ApiController]
[Route("api/rooms/{roomId:guid}/notes")]
public sealed class NotesController : ControllerBase
{
    private readonly ICommandHandler<CreateNoteCommand, NoteDto> _createNoteHandler;
    private readonly ICommandHandler<UpdateNoteCommand> _updateNoteHandler;
    private readonly ICommandHandler<DeleteNoteCommand> _deleteNoteHandler;
    private readonly IQueryHandler<GetRoomNotesQuery, IReadOnlyList<NoteDto>> _getRoomNotesHandler;

    public NotesController(
        ICommandHandler<CreateNoteCommand, NoteDto> createNoteHandler,
        ICommandHandler<UpdateNoteCommand> updateNoteHandler,
        ICommandHandler<DeleteNoteCommand> deleteNoteHandler,
        IQueryHandler<GetRoomNotesQuery, IReadOnlyList<NoteDto>> getRoomNotesHandler)
    {
        _createNoteHandler = createNoteHandler;
        _updateNoteHandler = updateNoteHandler;
        _deleteNoteHandler = deleteNoteHandler;
        _getRoomNotesHandler = getRoomNotesHandler;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NoteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoomNotes(
    [FromRoute] Guid roomId,
    [FromQuery] Guid playerId,
    [FromQuery] bool @private,
    CancellationToken ct)
    {
        var result = await _getRoomNotesHandler.HandleAsync(
            new GetRoomNotesQuery(roomId, playerId, @private),
            ct);

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Create(
        [FromRoute] Guid roomId,
        [FromBody] CreateNoteRequest request,
        CancellationToken ct)
    {
        var visibility = Enum.Parse<NoteVisibility>(request.Visibility, ignoreCase: true);

        await _createNoteHandler.HandleAsync(
            new CreateNoteCommand(
                roomId,
                request.Id,
                request.AuthorPlayerId,
                request.Content,
                visibility),
            ct);

        return NoContent();
    }

    [HttpPut("{noteId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid roomId,
        [FromRoute] Guid noteId,
        [FromBody] UpdateNoteRequest request,
        CancellationToken ct)
    {
        var visibility = Enum.Parse<NoteVisibility>(request.Visibility, ignoreCase: true);

        await _updateNoteHandler.HandleAsync(
            new UpdateNoteCommand(
                roomId,
                noteId,
                request.AuthorPlayerId,
                request.Content,
                visibility,
                "api"),
            ct);

        return NoContent();
    }

    [HttpDelete("{noteId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid roomId,
        [FromRoute] Guid noteId,
        CancellationToken ct)
    {
        await _deleteNoteHandler.HandleAsync(
            new DeleteNoteCommand(roomId, noteId),
            ct);

        return NoContent();
    }
}
```

---

## FILE: Kosmozeki.Application/Common/IReadDb.cs

```cs
using Kosmozeki.Contracts.Notes.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Application.Common;

public interface IReadDb
{
    Task<IReadOnlyList<NoteDto>> QueryRoomNotesAsync(Guid roomId, bool @private, CancellationToken ct);
    //Task<IReadOnlyList<ItemDto>> QueryRoomInventoryAsync(Guid roomId, CancellationToken ct);
    //Task<IReadOnlyList<ItemDto>> QueryPlayerInventoryAsync(Guid roomId, Guid playerId, CancellationToken ct);
    //Task<IReadOnlyList<ItemTransferLogDto>> QueryItemHistoryAsync(Guid itemId, CancellationToken ct);
    //Task<IReadOnlyList<PlayerDto>> QueryRoomPlayersAsync(Guid roomId, CancellationToken ct);
}

```

---

## FILE: Kosmozeki.Infrastructure/Persistence/Postgre/ReadDb.cs

_ERROR: file not found_

---

## FILE: Kosmozeki.Mobile/Services/Implementations/NotesFacade.cs

```cs
using Kosmozeki.Application.Common;
using Kosmozeki.Application.Notes.CreateNote;
using Kosmozeki.Application.Notes.DeleteNote;
using Kosmozeki.Application.Notes.GetRoomNotes;
using Kosmozeki.Application.Notes.UpdateNote;
using Kosmozeki.Contracts.Notes.Dtos;
using Kosmozeki.Domain.Notes;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Kosmozeki.Mobile.Services;

public sealed class NotesFacade
{
    private static readonly Guid DefaultRoomId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly IPlayerIdentity _playerIdentity;
    private readonly IQueryHandler<GetRoomNotesQuery, IReadOnlyList<NoteDto>> _getNotes;
    private readonly ICommandHandler<CreateNoteCommand, NoteDto> _createNote;
    private readonly ICommandHandler<UpdateNoteCommand> _updateNote;
    private readonly ICommandHandler<DeleteNoteCommand> _deleteNote;

    public NotesFacade(
        IPlayerIdentity playerIdentity,
        IQueryHandler<GetRoomNotesQuery, IReadOnlyList<NoteDto>> getNotes,
        ICommandHandler<CreateNoteCommand, NoteDto> createNote,
        ICommandHandler<UpdateNoteCommand> updateNote,
        ICommandHandler<DeleteNoteCommand> deleteNote)
    {
        _getNotes = getNotes;
        _createNote = createNote;
        _updateNote = updateNote;
        _deleteNote = deleteNote;
        _playerIdentity = playerIdentity;
    }

    public Task<IReadOnlyList<NoteDto>> GetNotesAsync(bool @private, CancellationToken ct = default)
    => _getNotes.HandleAsync(
        new GetRoomNotesQuery(DefaultRoomId, _playerIdentity.PlayerId, @private),
        ct);

    public Task<NoteDto> CreateAsync(string content, bool @private, CancellationToken ct = default)
        => _createNote.HandleAsync(
            new CreateNoteCommand(
                DefaultRoomId,
                Guid.NewGuid(),
                _playerIdentity.PlayerId,
                content,
                @private ? NoteVisibility.Private : NoteVisibility.Public,
                "maui"),
            ct);

    public Task UpdateAsync(Guid noteId, string content, bool @private, CancellationToken ct = default)
        => _updateNote.HandleAsync(
            new UpdateNoteCommand(
                DefaultRoomId,
                noteId,
                _playerIdentity.PlayerId,
                content,
                @private ? NoteVisibility.Private : NoteVisibility.Public),
            ct);

    public Task DeleteAsync(Guid noteId, CancellationToken ct = default)
        => _deleteNote.HandleAsync(
            new DeleteNoteCommand(DefaultRoomId, noteId),
            ct);
}
```

---

## FILE: Kosmozeki.Mobile/Services/Implementations/SyncBackgroundService.cs

```cs
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
        ILogger<SyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _roomContext = roomContext;
        _logger = logger;
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
        var remoteNotes = await transport.PullRoomNotesAsync(roomId, includePrivate: false, ct);

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


```

---

## FILE: Kosmozeki.Mobile/Services/Implementations/NotesSyncTransport.cs

```cs
using Kosmozeki.Api.Contracts.Notes;
using Kosmozeki.Contracts.Notes.Dtos;
using Kosmozeki.Mobile.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Kosmozeki.Mobile.Services;

public sealed class NotesSyncTransport : INotesSyncTransport
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public NotesSyncTransport(HttpClient httpClient, IOptions<ServerOptions> options)
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/')
            ?? throw new InvalidOperationException("ServerOptions.BaseUrl is not configured.");

        httpClient.BaseAddress = new Uri($"{baseUrl}/");
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<NoteDto>> PullRoomNotesAsync(
        Guid roomId,
        bool includePrivate,
        CancellationToken ct = default)
    {
        var url = $"api/rooms/{roomId:D}/notes?private={includePrivate.ToString().ToLowerInvariant()}";

        var result = await _httpClient.GetFromJsonAsync<IReadOnlyList<NoteDto>>(url, JsonOptions, ct);
        return result ?? Array.Empty<NoteDto>();
    }

    public async Task PushUpsertAsync(NoteDto note, CancellationToken ct = default)
    {
        if (note.IsDeleted)
        {
            await PushDeleteAsync(note.RoomId, note.Id, ct);
            return;
        }

        var request = new UpsertNoteRequest(
            note.Id,
            note.AuthorPlayerId,
            note.Content,
            note.Visibility,
            note.UpdatedAt);

        var response = await _httpClient.PutAsJsonAsync(
            $"api/rooms/{note.RoomId:D}/notes/{note.Id:D}",
            request,
            JsonOptions,
            ct);

        await EnsureSuccessAsync(response, ct);
    }

    public async Task PushDeleteAsync(
        Guid roomId,
        Guid noteId,
        CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync(
            $"api/rooms/{roomId:D}/notes/{noteId:D}",
            ct);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return;

        await EnsureSuccessAsync(response, ct);
    }

    private async Task<bool> ExistsAsync(
        Guid roomId,
        Guid noteId,
        CancellationToken ct)
    {
        var notes = await PullRoomNotesAsync(roomId, includePrivate: true, ct);
        return notes.Any(x => x.Id == noteId);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = response.Content is null
            ? null
            : await response.Content.ReadAsStringAsync(ct);

        throw new HttpRequestException(
            $"Notes sync request failed: {(int)response.StatusCode} {response.ReasonPhrase}. {body}");
    }
}
```

---

## FILE: Kosmozeki.Mobile/Components/Pages/Notes.razor

```razor
@page "/notes"
@using Kosmozeki.Contracts.Notes.Dtos
@using Kosmozeki.Mobile.Services
@using Kosmozeki.Core.Realtime
@implements IAsyncDisposable
@inject NotesFacade NotesFacade
@inject IRoomRealtimeService RoomRealtimeService
@inject ISyncBackgroundService SyncBackgroundService


<PageTitle>Notes</PageTitle>

<div class="mb-3">
    <label class="form-label">Новая заметка</label>
    <textarea class="form-control" rows="5" @bind="_newContent" disabled="@_isBusy"></textarea>
</div>

<div class="form-check mb-3">
    <input class="form-check-input" type="checkbox" id="privateNew" @bind="_newPrivate" disabled="@_isBusy" />
    <label class="form-check-label" for="privateNew">Приватное</label>
</div>

<div class="mb-4">
    <button class="btn btn-primary me-2" @onclick="CreateNoteAsync" disabled="@_isBusy">Сохранить</button>
    <button class="btn btn-secondary" @onclick="RefreshAsync" disabled="@_isBusy">Обновить</button>
</div>

@if (!string.IsNullOrWhiteSpace(_status))
{
    <div class="alert alert-info">@_status</div>
}

@if (_isBusy && _notes.Count == 0)
{
    <p>Загрузка...</p>
}
else if (_notes.Count == 0)
{
    <p>Заметок пока нет.</p>
}
else
{
    @foreach (var note in _notes)
    {
        var isEditing = _editId == note.Id;

        <div class="card mb-3">
            <div class="card-body">
                @if (isEditing)
                {
                    <div class="mb-2">
                        <textarea class="form-control" rows="5" @bind="_editContent" disabled="@_isBusy"></textarea>
                    </div>

                    <div class="form-check mb-3">
                        <input class="form-check-input"
                               type="checkbox"
                               id="@($"private-{note.Id}")"
                               @bind="_editPrivate"
                               disabled="@_isBusy" />
                        <label class="form-check-label" for="@($"private-{note.Id}")">Приватное</label>
                    </div>

                    <button class="btn btn-success me-2" @onclick="() => SaveEditAsync(note.Id)" disabled="@_isBusy">Применить</button>
                    <button class="btn btn-outline-secondary" @onclick="CancelEdit" disabled="@_isBusy">Отмена</button>
                }
                else
                {
                    <div class="d-flex justify-content-between align-items-start mb-2">
                        <div class="d-flex gap-2 align-items-center flex-wrap">
                            <span class="badge @(string.Equals(note.Visibility, "Private", StringComparison.OrdinalIgnoreCase)
                                                                  ? "bg-warning text-dark"
                                                                  : "bg-info text-dark")">
                                @note.Visibility
                            </span>

                            <small class="text-muted">
                                @note.UpdatedAt.LocalDateTime.ToString("g")
                            </small>
                        </div>
                    </div>

                    <pre style="white-space: pre-wrap; margin: 0;">@note.Content</pre>

                    <div class="mt-3">
                        <button class="btn btn-sm btn-outline-primary me-2" @onclick="() => BeginEdit(note)" disabled="@_isBusy">Редактировать</button>
                        <button class="btn btn-sm btn-outline-danger" @onclick="() => DeleteAsync(note.Id)" disabled="@_isBusy">Удалить</button>
                    </div>
                }
            </div>
        </div>
    }
}

@code {
    private readonly List<NoteDto> _notes = [];
    private bool _isBusy;
    private string _status = string.Empty;

    private string _newContent = string.Empty;
    private bool _newPrivate;

    private Guid? _editId;
    private string _editContent = string.Empty;
    private bool _editPrivate;

    private static readonly Guid RoomId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    protected override async Task OnInitializedAsync()
    {
        SyncBackgroundService.SyncCompleted += HandleSyncCompletedAsync;
        SyncBackgroundService.SyncFailed += HandleSyncFailedAsync;
        RoomRealtimeService.NotesChanged += HandleNotesChangedAsync;

        await SyncBackgroundService.StartAsync();

        try
        {
            await RoomRealtimeService.StartAsync(RoomId);
        }
        catch (Exception ex)
        {
            _status = $"Realtime недоступен: {ex.Message}";
        }

        await ReloadLocalAsync(clearStatus: false);
        await SyncBackgroundService.TrySyncAsync();
        await ReloadLocalAsync(clearStatus: false);
    }

    private async Task HandleNotesChangedAsync(Guid roomId)
    {
        if (roomId != RoomId)
            return;

        await SyncBackgroundService.TrySyncAsync();
        await ReloadLocalAsync(clearStatus: false);

        _status = "Получены изменения с сервера.";
        await InvokeAsync(StateHasChanged);
    }

    private async Task HandleSyncCompletedAsync()
    {
        await ReloadLocalAsync(clearStatus: false);
        _status = "Синхронизация завершена.";
        await InvokeAsync(StateHasChanged);
    }

    private Task HandleSyncFailedAsync(string message)
    {
        _status = $"Синхронизация отложена: {message}";
        return InvokeAsync(StateHasChanged);
    }

    private async Task RefreshAsync()
    {
        if (_isBusy)
            return;

        await ReloadLocalAsync(clearStatus: false);
        _status = "Локальные данные обновлены.";

        _ = SyncBackgroundService.TrySyncAsync();
    }

    private async Task ReloadLocalAsync(bool clearStatus = true)
    {
        try
        {
            _isBusy = true;

            if (clearStatus)
                _status = string.Empty;

            var notes = await NotesFacade.GetNotesAsync(false);

            _notes.Clear();
            _notes.AddRange(notes
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.UpdatedAt));

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            _status = $"Ошибка загрузки: {ex.Message}";
        }
        finally
        {
            _isBusy = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task CreateNoteAsync()
    {
        if (string.IsNullOrWhiteSpace(_newContent))
        {
            _status = "Введите текст заметки.";
            return;
        }

        try
        {
            _isBusy = true;
            _status = string.Empty;

            await NotesFacade.CreateAsync(_newContent.Trim(), _newPrivate);

            _newContent = string.Empty;
            _newPrivate = false;

            await ReloadLocalAsync(clearStatus: false);
            _status = "Заметка сохранена локально.";

            _ = SyncBackgroundService.TrySyncAsync();
        }
        catch (Exception ex)
        {
            _status = $"Ошибка создания: {ex.Message}";
        }
        finally
        {
            _isBusy = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private void BeginEdit(NoteDto note)
    {
        _editId = note.Id;
        _editContent = note.Content;
        _editPrivate = string.Equals(note.Visibility, "Private", StringComparison.OrdinalIgnoreCase);
        _status = string.Empty;
    }

    private void CancelEdit()
    {
        _editId = null;
        _editContent = string.Empty;
        _editPrivate = false;
    }

    private async Task SaveEditAsync(Guid noteId)
    {
        if (string.IsNullOrWhiteSpace(_editContent))
        {
            _status = "Текст заметки не может быть пустым.";
            return;
        }

        try
        {
            _isBusy = true;
            _status = string.Empty;

            await NotesFacade.UpdateAsync(noteId, _editContent.Trim(), _editPrivate);

            CancelEdit();
            await ReloadLocalAsync(clearStatus: false);
            _status = "Заметка обновлена локально.";

            _ = SyncBackgroundService.TrySyncAsync();
        }
        catch (Exception ex)
        {
            _status = $"Ошибка обновления: {ex.Message}";
        }
        finally
        {
            _isBusy = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task DeleteAsync(Guid noteId)
    {
        try
        {
            _isBusy = true;
            _status = string.Empty;

            await NotesFacade.DeleteAsync(noteId);

            if (_editId == noteId)
                CancelEdit();

            await ReloadLocalAsync(clearStatus: false);
            _status = "Заметка удалена локально.";

            _ = SyncBackgroundService.TrySyncAsync();
        }
        catch (Exception ex)
        {
            _status = $"Ошибка удаления: {ex.Message}";
        }
        finally
        {
            _isBusy = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    public async ValueTask DisposeAsync()
    {
        SyncBackgroundService.SyncCompleted -= HandleSyncCompletedAsync;
        SyncBackgroundService.SyncFailed -= HandleSyncFailedAsync;
        RoomRealtimeService.NotesChanged -= HandleNotesChangedAsync;

        await RoomRealtimeService.StopAsync(RoomId);
    }
} 
```

