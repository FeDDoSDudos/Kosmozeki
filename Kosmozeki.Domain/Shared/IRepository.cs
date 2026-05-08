namespace Kosmozeki.Domain.Shared;


public interface IRepository<T> where T : AggregateRoot
{

    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task UpsertAsync(T aggregate, CancellationToken cancellationToken = default);
    
}