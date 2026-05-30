namespace Kosmozeki.Api.Contracts.Notes;

public sealed record UpdateNoteRequest(
    string Content,
    string Visibility);