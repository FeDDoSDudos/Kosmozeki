using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Contracts.Notes;

public sealed record OutboxNotePayload(
    Guid Id,
    Guid RoomId,
    Guid AuthorPlayerId,
    string Content,
    string Visibility,
    DateTimeOffset UpdatedAt,
    bool IsDeleted);
