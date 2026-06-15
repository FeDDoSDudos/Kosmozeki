using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Application.Notes;

public static class NotesCacheKeys
{
    public static string Room(Guid roomId)
        => $"notes:room:{roomId}";
}