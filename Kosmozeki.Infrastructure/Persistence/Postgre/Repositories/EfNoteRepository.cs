using Kosmozeki.Domain.Notes;
using Microsoft.EntityFrameworkCore;

namespace Kosmozeki.Infrastructure.Persistence.Postgre.Repositories;

public sealed class EfNoteRepository : INoteRepository
{
    private readonly AppDbContext _dbContext;

    public EfNoteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SharedNote?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _dbContext.Notes.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task UpsertAsync(SharedNote note, CancellationToken ct = default)
    {
        var existing = await _dbContext.Notes
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == note.Id, ct);

        if (existing is null)
        {
            await _dbContext.Notes.AddAsync(note, ct);
            return;
        }

        _dbContext.Entry(existing).CurrentValues.SetValues(note);
    }
}