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
