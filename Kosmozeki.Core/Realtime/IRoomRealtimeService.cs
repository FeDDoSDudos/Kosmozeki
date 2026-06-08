using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Core.Realtime;

public interface IRoomRealtimeService : IAsyncDisposable
{
    event Func<Guid, Task>? NotesChanged;
    Task StartAsync(Guid roomId, CancellationToken ct = default);
    Task StopAsync(Guid roomId, CancellationToken ct = default);
}
