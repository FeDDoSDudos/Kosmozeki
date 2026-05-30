using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Application.Notes.DeleteNote;

public sealed record DeleteNoteCommand(
    Guid RoomId,
    Guid NoteId);
