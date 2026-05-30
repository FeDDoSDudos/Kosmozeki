using Kosmozeki.Domain.Shared;
using Kosmozeki.Domain.Sync;
using Microsoft.EntityFrameworkCore;

namespace Kosmozeki.Infrastructure.Persistence.Postgre.Repositories;

public sealed class EfOutboxRepository : IOutboxRepository
{
    private readonly AppDbContext _dbContext;

    public EfOutboxRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(OutboxEntry entry, CancellationToken ct = default)
    {
        await _dbContext.OutboxEntries.AddAsync(entry, ct);
    }

    public async Task<IReadOnlyList<OutboxEntry>> GetPendingAsync(int take, CancellationToken ct = default)
    {
        return await _dbContext.OutboxEntries
            .AsNoTracking()
            .Where(x => x.ProcessedAt == null)
            .OrderBy(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task MarkProcessedAsync(Guid id, CancellationToken ct = default)
    {
        var entry = await _dbContext.OutboxEntries.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entry is null)
            return;

        entry.MarkAsProcessed();
    }
}