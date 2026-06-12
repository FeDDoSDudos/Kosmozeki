# Exported project files

Root: C:\Users\frolo\source\repos\Kosmozeki
Generated: 2026-06-12 12:38:22

---

## FILE: Kosmozeki.Application/Notes/CreateNote/CreateNoteCommand.cs

```cs
using Kosmozeki.Domain.Notes;

namespace Kosmozeki.Application.Notes.CreateNote;

public sealed record CreateNoteCommand(
    Guid RoomId,
    Guid Id,
    Guid AuthorPlayerId,
    string Content,
    NoteVisibility Visibility,
    string? LastModifiedBy = null);
```

---

## FILE: Kosmozeki.Application/Notes/UpdateNote/UpdateNoteCommand.cs

```cs
using Kosmozeki.Domain.Notes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Application.Notes.UpdateNote;

public sealed record UpdateNoteCommand(
    Guid RoomId,
    Guid NoteId,
    string Content,
    NoteVisibility Visibility);

```

---

## FILE: Kosmozeki.Application/Notes/CreateNote/CreateNoteCommandHandler.cs

```cs
using Kosmozeki.Application.Common;
using Kosmozeki.Contracts.Notes.Dtos;
using Kosmozeki.Domain.Notes;
using Kosmozeki.Domain.Shared;
using Kosmozeki.Domain.Sync;

namespace Kosmozeki.Application.Notes.CreateNote;

public sealed class CreateNoteCommandHandler
    : ICommandHandler<CreateNoteCommand, NoteDto>
{
    private readonly INoteRepository _notes;
    private readonly IOutboxRepository _outbox;
    private readonly IUnitOfWork _uow;
    private readonly ICache _cache;
    private readonly IDomainEventDispatcher _events;

    public CreateNoteCommandHandler(
        INoteRepository notes,
        IOutboxRepository outbox,
        IUnitOfWork uow,
        ICache cache,
        IDomainEventDispatcher events)
    {
        _notes = notes;
        _outbox = outbox;
        _uow = uow;
        _cache = cache;
        _events = events;
    }

    public async Task<NoteDto> HandleAsync(CreateNoteCommand command, CancellationToken ct = default)
    {
        await _uow.BeginAsync(ct);

        try
        {
            var note = SharedNote.Create(
                command.Id,
                command.RoomId,
                command.AuthorPlayerId,
                command.Content,
                command.Visibility,
                command.LastModifiedBy);

            await _notes.UpsertAsync(note, ct);
            await _outbox.AddAsync(OutboxEntry.From(note), ct);
            await _uow.CommitAsync(ct);

            await _cache.RemoveAsync(NotesCacheKeys.Room(command.RoomId), ct);
            await _events.DispatchAsync(note.DomainEvents, ct);
            note.ClearDomainEvents();

            return NoteMapper.ToDto(note, authorName: null, authorAvatarUrl: null);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}

```

---

## FILE: Kosmozeki.Application/Notes/UpdateNote/UpdateNoteCommandHandler.cs

```cs
using Kosmozeki.Application.Common;
using Kosmozeki.Domain.Notes;
using Kosmozeki.Domain.Shared;
using Kosmozeki.Domain.Sync;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Application.Notes.UpdateNote;

public sealed class UpdateNoteCommandHandler : ICommandHandler<UpdateNoteCommand>
{
    private readonly INoteRepository _notes;
    private readonly IOutboxRepository _outbox;
    private readonly IUnitOfWork _uow;
    private readonly ICache _cache;
    private readonly IDomainEventDispatcher _events;

    public UpdateNoteCommandHandler(
        INoteRepository notes,
        IOutboxRepository outbox,
        IUnitOfWork uow,
        ICache cache,
        IDomainEventDispatcher events)
    {
        _notes = notes;
        _outbox = outbox;
        _uow = uow;
        _cache = cache;
        _events = events;
    }

    public async Task HandleAsync(UpdateNoteCommand command, CancellationToken ct = default)
    {
        await _uow.BeginAsync(ct);

        SharedNote? note;

        try
        {
            note = await _notes.GetByIdAsync(command.NoteId, ct);
            if (note is null)
                throw new InvalidOperationException($"Note '{command.NoteId}' was not found.");

            note.Update(command.Content, command.Visibility);

            await _notes.UpsertAsync(note, ct);
            await _outbox.AddAsync(OutboxEntry.From(note), ct);

            await _uow.CommitAsync(ct);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }

        await _cache.RemoveAsync(NotesCacheKeys.Room(command.RoomId), ct);
        await _events.DispatchAsync(note.DomainEvents, ct);
        note.ClearDomainEvents();
    }
}

```

---

## FILE: Kosmozeki.Domain/Notes/SharedNote.cs

```cs
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
        Guid id,
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
            Id = id,
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

## FILE: Kosmozeki.Api/Contracts/Notes/CreateNoteRequest.cs

_ERROR: file not found_

---

## FILE: Kosmozeki.Api/Contracts/Notes/UpsertNoteRequest.cs

_ERROR: file not found_

