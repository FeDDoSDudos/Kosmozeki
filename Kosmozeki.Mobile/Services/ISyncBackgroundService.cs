using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Mobile.Services;

public interface ISyncBackgroundService
{
    event Func<Task>? SyncCompleted;
    event Func<string, Task>? SyncFailed;

    Task StartAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
    Task TrySyncAsync(CancellationToken ct = default);
}
