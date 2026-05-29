namespace Kosmozeki.Contracts.Notes.Dtos;

public sealed record NoteDto(
    Guid Id,
    Guid RoomId,
    string Content,
    Guid AuthorPlayerId,
    string? AuthorName,
    string? AuthorAvatarUrl,
    string Visibility,
    DateTimeOffset UpdatedAt,
    bool IsDeleted);