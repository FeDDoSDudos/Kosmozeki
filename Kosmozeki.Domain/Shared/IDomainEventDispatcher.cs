
namespace Kosmozeki.Domain.Shared;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);
}