using System.ComponentModel.DataAnnotations;

namespace Kosmozeki.Domain.Shared;

public abstract class DomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}