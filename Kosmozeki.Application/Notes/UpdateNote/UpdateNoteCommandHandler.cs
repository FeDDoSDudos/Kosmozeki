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

        await _cache.RemoveByPrefixAsync($"notesroom{command.RoomId}", ct);
        await _events.DispatchAsync(note.DomainEvents, ct);
        note.ClearDomainEvents();
    }
}
