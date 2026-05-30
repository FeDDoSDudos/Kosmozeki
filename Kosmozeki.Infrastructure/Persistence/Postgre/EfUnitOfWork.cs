using Kosmozeki.Domain.Shared;
using Microsoft.EntityFrameworkCore.Storage;

namespace Kosmozeki.Infrastructure.Persistence.Postgre;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;
    private IDbContextTransaction? _transaction;

    public EfUnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task BeginAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
            return;

        _transaction = await _dbContext.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        await _dbContext.SaveChangesAsync(ct);

        if (_transaction is not null)
        {
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
            return;

        await _transaction.RollbackAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }
}