using System.ComponentModel.DataAnnotations.Schema;

namespace Kosmozeki.Domain.Shared;

public abstract class AggregateRoot
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    [NotMapped]
    private readonly List<DomainEvent> _domainEvents = new();
    
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void RaiseDomainEvent(DomainEvent e) => _domainEvents.Add(e);
    
    public void ClearDomainEvents() => _domainEvents.Clear();
}