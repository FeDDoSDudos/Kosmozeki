using Kosmozeki.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Domain.Sync;

public interface IOutboxRepository
{
    Task AddAsync(OutboxEntry entry, CancellationToken ct = default);
    Task<IReadOnlyList<OutboxEntry>> GetPendingAsync(int take, CancellationToken ct = default);
    Task MarkProcessedAsync(Guid id, CancellationToken ct = default);
}
