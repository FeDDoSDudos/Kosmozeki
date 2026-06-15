using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Application.Notes.GetRoomNotes;

public sealed record GetRoomNotesQuery(
    Guid RoomId,
    Guid CurrentPlayerId,
    bool IncludePrivate = false);
