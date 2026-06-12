using Kosmozeki.Domain.Notes;

namespace Kosmozeki.Application.Notes.CreateNote;

public sealed record CreateNoteCommand(
    Guid RoomId,
    Guid Id,
    Guid AuthorPlayerId,
    string Content,
    NoteVisibility Visibility,
    string? LastModifiedBy = null);