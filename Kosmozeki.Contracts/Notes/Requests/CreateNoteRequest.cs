namespace Kosmozeki.Api.Contracts.Notes;

public sealed record CreateNoteRequest(
    Guid AuthorPlayerId,
    string Content,
    string Visibility);
