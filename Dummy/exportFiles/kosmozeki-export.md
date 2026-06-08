# Exported project files

Root: C:\Users\frolo\source\repos\Kosmozeki
Generated: 2026-06-08 00:21:26

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
        [FromQuery] bool @private,
        CancellationToken ct)
    {
        var result = await _getRoomNotesHandler.HandleAsync(
            new GetRoomNotesQuery(roomId, @private),
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
                request.Content,
                visibility),
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

## FILE: Kosmozeki.Api/Realtime/SignalRRoomEventsPublisher.cs

```cs
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

```

---

## FILE: Kosmozeki.Application/Common/IRoomEventsPublisher.cs

```cs
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Application.Common;

public interface IRoomEventsPublisher
{
    Task PublishNotesChangedAsync(
        Guid roomId,
        Guid noteId,
        string type,
        CancellationToken ct = default);
}

```

---

## FILE: Kosmozeki.Contracts/Notes/Events/NoteChangedEvent.cs

```cs
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Contracts.Notes.Events
{
    internal class NoteChangedEvent
    {
    }
}

```

---

## FILE: Kosmozeki.Contracts/Notes/Events/NoteDeletedEvent.cs

```cs
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Contracts.Notes.Events
{
    internal class NoteDeletedEvent
    {
    }
}

```

---

## FILE: Kosmozeki.Contracts/Sync/Dtos/SyncRequestDto.cs

```cs
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Contracts.Sync.Dtos
{
    internal class SyncRequestDto
    {
    }
}

```

---

## FILE: Kosmozeki.Contracts/Sync/Dtos/SyncResponseDto.cs

```cs
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Contracts.Sync.Dtos
{
    internal class SyncResponseDto
    {
    }
}

```

---

## FILE: Kosmozeki.Core/Realtime/Implementations/RoomRealtimeService.cs

```cs
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
        var baseUrl = options.Value.BaseUrl.TrimEnd('/');
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
                if (_joinedRoomId.HasValue)
                    await _connection.InvokeAsync("JoinRoom", _joinedRoomId.Value.ToString());
            };

            await _connection.StartAsync(ct);
        }

        if (_joinedRoomId == roomId)
            return;

        if (_joinedRoomId.HasValue)
            await _connection.InvokeAsync("LeaveRoom", _joinedRoomId.Value.ToString(), ct);

        await _connection.InvokeAsync("JoinRoom", roomId.ToString(), ct);
        _joinedRoomId = roomId;
    }

    public async Task StopAsync(Guid roomId, CancellationToken ct = default)
    {
        if (_connection is null)
            return;

        if (_joinedRoomId == roomId)
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

    private sealed record NotesChangedMessage(Guid RoomId, Guid NoteId, string Type, DateTimeOffset OccurredAt);
}
```

---

## FILE: Kosmozeki.Mobile/MauiProgram.cs

```cs
using Kosmozeki.Application.DependencyInjection;
using Kosmozeki.Core.Realtime;
using Kosmozeki.Core.Realtime.Implementations;
using Kosmozeki.Core.Services;
using Kosmozeki.Domain.Shared;
using Kosmozeki.Infrastructure.DependencyInjection;
using Kosmozeki.Infrastructure.Messaging;
using Kosmozeki.Mobile.Options;
using Kosmozeki.Mobile.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kosmozeki.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });


        using var appSettingsStream = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult();
        builder.Configuration.AddJsonStream(appSettingsStream);
        builder.Services.Configure<ServerOptions>(builder.Configuration.GetSection(ServerOptions.SectionName));

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "kosmozeki.db");

        builder.Services.AddMauiBlazorWebView();

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure();
        builder.Services.AddCache();
        builder.Services.AddKosmozekiMauiInfrastructure(dbPath);

        builder.Services.AddScoped<CombatFacade>();
        builder.Services.AddScoped<NotesFacade>();

        builder.Services.AddSingleton<CombatEngineService>();

        builder.Services.AddSingleton<IRoomRealtimeService, RoomRealtimeService>();
        builder.Services.AddSingleton<IRoomContext, RoomContext>();
        builder.Services.AddSingleton<ISyncBackgroundService, SyncBackgroundService>();
        builder.Services.AddHttpClient<INotesSyncTransport, NotesSyncTransport>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ServerOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        builder.Services.AddScoped<IDomainEventDispatcher, NoOpDomainEventDispatcher>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
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
        await ReloadLocalAsync(clearStatus: false);
        _status = "Локальные данные загружены.";

        _ = SyncBackgroundService.TrySyncAsync();

        try
        {
            await RoomRealtimeService.StartAsync(RoomId);
        }
        catch (Exception ex)
        {
            _status = $"Realtime недоступен, работаем без live-обновлений: {ex.Message}";
            await InvokeAsync(StateHasChanged);
        }
    }

    private Task HandleNotesChangedAsync(Guid roomId)
    {
        if (roomId != RoomId)
            return Task.CompletedTask;

        _ = SyncBackgroundService.TrySyncAsync();
        return Task.CompletedTask;
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

---

## FILE: Kosmozeki.Core/Services/ApiClients/INotesSyncTransport.cs

_ERROR: file not found_

---

## FILE: Kosmozeki.Core/Services/ApiClients/Implementations/NotesSyncTransport.cs

_ERROR: file not found_

---

## FILE: Kosmozeki.Mobile/Services/SyncBackgroundService.cs

_ERROR: file not found_

---

## FILE: Kosmozeki.Mobile/Services/NotesFacade.cs

_ERROR: file not found_

---

## FILE: Kosmozeki.Infrastructure/ReadDb/SqliteReadDb.cs

```cs
using Kosmozeki.Application.Common;
using Kosmozeki.Contracts.Notes.Dtos;
using Microsoft.Data.Sqlite;

namespace Kosmozeki.Infrastructure.ReadDb;

public sealed class SqliteReadDb : IReadDb
{
    private readonly SqliteConnection _connection;

    public SqliteReadDb(SqliteConnection connection)
    {
        _connection = connection;
    }

    public async Task<IReadOnlyList<NoteDto>> QueryRoomNotesAsync(
        Guid roomId,
        bool @private,
        CancellationToken ct)
    {
        if (_connection.State != System.Data.ConnectionState.Open)
            await _connection.OpenAsync(ct);

        var result = new List<NoteDto>();

        await using var command = _connection.CreateCommand();
        command.CommandText = """
            select
                Id,
                RoomId,
                Content,
                AuthorPlayerId,
                Visibility,
                UpdatedAt,
                IsDeleted
            from Notes
            where RoomId = $roomId
              and IsDeleted = 0
              and ($private = 0 or Visibility = 'Private')
            order by UpdatedAt desc;
            """;

        command.Parameters.AddWithValue("$roomId", roomId.ToString());
        command.Parameters.AddWithValue("$private", @private ? 1 : 0);

        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(new NoteDto(
                Guid.Parse(reader.GetString(0)),
                Guid.Parse(reader.GetString(1)),
                reader.GetString(2),
                Guid.Parse(reader.GetString(3)),
                null,
                null,
                reader.GetString(4),
                DateTimeOffset.Parse(reader.GetString(5)),
                reader.GetInt64(6) == 1));
        }

        return result;
    }

    //public Task<IReadOnlyList<ItemDto>> QueryRoomInventoryAsync(Guid roomId, CancellationToken ct)
    //    => Task.FromResult<IReadOnlyList<ItemDto>>(Array.Empty<ItemDto>());

    //public Task<IReadOnlyList<ItemDto>> QueryPlayerInventoryAsync(Guid roomId, Guid playerId, CancellationToken ct)
    //    => Task.FromResult<IReadOnlyList<ItemDto>>(Array.Empty<ItemDto>());

    //public Task<IReadOnlyList<ItemTransferLogDto>> QueryItemHistoryAsync(Guid itemId, CancellationToken ct)
    //    => Task.FromResult<IReadOnlyList<ItemTransferLogDto>>(Array.Empty<ItemTransferLogDto>());

    //public Task<IReadOnlyList<PlayerDto>> QueryRoomPlayersAsync(Guid roomId, CancellationToken ct)
    //    => Task.FromResult<IReadOnlyList<PlayerDto>>(Array.Empty<PlayerDto>());
}
```

---

## FILE: Kosmozeki.Infrastructure/ReadDb/PostgresReadDb.cs

```cs
using Kosmozeki.Application.Common;
using Kosmozeki.Contracts.Notes.Dtos;
using Kosmozeki.Domain.Notes;
using Kosmozeki.Infrastructure.Persistence.Postgre;
using Microsoft.EntityFrameworkCore;

namespace Kosmozeki.Infrastructure.ReadDb;

public sealed class PostgresReadDb : IReadDb
{
    private readonly AppDbContext _dbContext;

    public PostgresReadDb(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<NoteDto>> QueryRoomNotesAsync(
        Guid roomId,
        bool @private,
        CancellationToken ct)
    {
        var query = _dbContext.Notes
            .AsNoTracking()
            .Where(x => x.RoomId == roomId && !x.IsDeleted);

        if (@private)
        {
            query = query.Where(x => x.Visibility == NoteVisibility.Private);
        }

        return await query
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new NoteDto(
                x.Id,
                x.RoomId,
                x.Content,
                x.AuthorPlayerId,
                null,
                null,
                x.Visibility.ToString(),
                x.UpdatedAt,
                x.IsDeleted))
            .ToListAsync(ct);
    }

    //public Task<IReadOnlyList<ItemDto>> QueryRoomInventoryAsync(Guid roomId, CancellationToken ct)
    //    => Task.FromResult<IReadOnlyList<ItemDto>>(Array.Empty<ItemDto>());

    //public Task<IReadOnlyList<ItemDto>> QueryPlayerInventoryAsync(Guid roomId, Guid playerId, CancellationToken ct)
    //    => Task.FromResult<IReadOnlyList<ItemDto>>(Array.Empty<ItemDto>());

    //public Task<IReadOnlyList<ItemTransferLogDto>> QueryItemHistoryAsync(Guid itemId, CancellationToken ct)
    //    => Task.FromResult<IReadOnlyList<ItemTransferLogDto>>(Array.Empty<ItemTransferLogDto>());

    //public Task<IReadOnlyList<PlayerDto>> QueryRoomPlayersAsync(Guid roomId, CancellationToken ct)
    //    => Task.FromResult<IReadOnlyList<PlayerDto>>(Array.Empty<PlayerDto>());
}
```

