
namespace Kosmozeki.Domain.Shared;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<DomainEvent> events, CancellationToken ct = default);
}