namespace Kosmozeki.Domain.Shared;

public interface IUnitOfWork
{
    Task BeginAsync(CancellationToken cancellationToken = default);
    
    Task CommitAsync(CancellationToken cancellationToken = default);
    
    Task RollbackAsync(CancellationToken cancellationToken = default);
}