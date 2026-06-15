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