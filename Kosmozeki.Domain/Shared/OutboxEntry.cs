namespace Kosmozeki.Domain.Shared;

public sealed class OutboxEntry
{
    public Guid Id { get; private set; }
    
    public string EventType { get; private set; }
    
    public string EventData { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset? ProcessedAt { get; private set; }

    public OutboxEntry(string eventType, string eventData)
    {
        Id = Guid.NewGuid();
        EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
        EventData = eventData ?? throw new ArgumentNullException(nameof(eventData));
        CreatedAt = DateTimeOffset.UtcNow;
        ProcessedAt = null;
    }
    
    public void MarkAsProcessed()
    {
        ProcessedAt = DateTimeOffset.UtcNow;
    }
    
    private OutboxEntry() { }
}