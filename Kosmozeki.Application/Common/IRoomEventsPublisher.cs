using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Application.Common;

public interface IRoomEventsPublisher
{
    Task PublishNotesChangedAsync(
        Guid roomId,
        Guid noteId,
        string type,
        CancellationToken ct = default);
}
