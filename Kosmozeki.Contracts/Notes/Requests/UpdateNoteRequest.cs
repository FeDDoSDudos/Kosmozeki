namespace Kosmozeki.Api.Contracts.Notes;

public sealed record UpdateNoteRequest(
    Guid AuthorPlayerId,
    string Content,
    string Visibility,
    DateTimeOffset UpdatedAt);