namespace Kosmozeki.Api.Contracts.Notes;

public sealed record UpsertNoteRequest(
    Guid Id,
    Guid AuthorPlayerId,
    string Content,
    string Visibility,
    DateTimeOffset UpdatedAt);