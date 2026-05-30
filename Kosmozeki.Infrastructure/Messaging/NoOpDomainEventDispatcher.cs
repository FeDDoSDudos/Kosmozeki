using Kosmozeki.Domain.Shared;

namespace Kosmozeki.Infrastructure.Messaging;

public sealed class NoOpDomainEventDispatcher : IDomainEventDispatcher
{
    public Task DispatchAsync(IEnumerable<DomainEvent> events, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}