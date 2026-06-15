namespace Kosmozeki.Api.Contracts.Notes;

public sealed record CreateNoteRequest(
    Guid Id,
    Guid AuthorPlayerId,
    string Content,
    string Visibility);
