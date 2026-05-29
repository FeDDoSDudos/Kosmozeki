using Kosmozeki.Domain.Notes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Application.Notes.UpdateNote;

public sealed record UpdateNoteCommand(
    Guid RoomId,
    Guid NoteId,
    string Content,
    NoteVisibility Visibility);
