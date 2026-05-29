using System.Text.Json;

namespace Kosmozeki.Domain.Shared;

public sealed class OutboxEntry
{
    public Guid Id { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string Operation { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public int RetryCount { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    private OutboxEntry() { }
    
    public void MarkAsProcessed()
    {
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    public static OutboxEntry From(SyncableEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new OutboxEntry
        {
            Id = Guid.NewGuid(),
            EntityType = entity.GetType().Name,
            EntityId = entity.Id,
            Operation = entity.IsDeleted ? "delete" : "upsert",
            Payload = JsonSerializer.Serialize(entity, entity.GetType()),
            RetryCount = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            ProcessedAt = null
        };
    }

    public void IncrementRetry()
    {
        RetryCount++;
    }
}