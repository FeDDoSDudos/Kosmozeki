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
    private static readonly Guid DefaultAuthorId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly IQueryHandler<GetRoomNotesQuery, IReadOnlyList<NoteDto>> _getNotes;
    private readonly ICommandHandler<CreateNoteCommand, NoteDto> _createNote;
    private readonly ICommandHandler<UpdateNoteCommand> _updateNote;
    private readonly ICommandHandler<DeleteNoteCommand> _deleteNote;

    public NotesFacade(
        IQueryHandler<GetRoomNotesQuery, IReadOnlyList<NoteDto>> getNotes,
        ICommandHandler<CreateNoteCommand, NoteDto> createNote,
        ICommandHandler<UpdateNoteCommand> updateNote,
        ICommandHandler<DeleteNoteCommand> deleteNote)
    {
        _getNotes = getNotes;
        _createNote = createNote;
        _updateNote = updateNote;
        _deleteNote = deleteNote;
    }

    public Task<IReadOnlyList<NoteDto>> GetNotesAsync(bool @private, CancellationToken ct = default)
    => _getNotes.HandleAsync(
        new GetRoomNotesQuery(DefaultRoomId, DefaultAuthorId, @private),
        ct);

    public Task<NoteDto> CreateAsync(string content, bool @private, CancellationToken ct = default)
        => _createNote.HandleAsync(
            new CreateNoteCommand(
                DefaultRoomId,
                Guid.NewGuid(),
                DefaultAuthorId,
                content,
                @private ? NoteVisibility.Private : NoteVisibility.Public,
                "maui"),
            ct);

    public Task UpdateAsync(Guid noteId, string content, bool @private, CancellationToken ct = default)
        => _updateNote.HandleAsync(
            new UpdateNoteCommand(
                DefaultRoomId,
                noteId,
                DefaultAuthorId,
                content,
                @private ? NoteVisibility.Private : NoteVisibility.Public),
            ct);

    public Task DeleteAsync(Guid noteId, CancellationToken ct = default)
        => _deleteNote.HandleAsync(
            new DeleteNoteCommand(DefaultRoomId, noteId),
            ct);
}