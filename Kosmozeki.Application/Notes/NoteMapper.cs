using Kosmozeki.Contracts.Notes.Dtos;
using Kosmozeki.Domain.Notes;

namespace Kosmozeki.Application.Notes;

public static class NoteMapper
{
    public static NoteDto ToDto(
        SharedNote note,
        string? authorName,
        string? authorAvatarUrl)
    {
        return new NoteDto(
            note.Id,
            note.RoomId,
            note.Content,
            note.AuthorPlayerId,
            authorName,
            authorAvatarUrl,
            note.Visibility.ToString(),
            note.UpdatedAt,
            note.IsDeleted);
    }
}