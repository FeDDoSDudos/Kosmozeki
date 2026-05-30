namespace Kosmozeki.Domain.Shared;


public abstract class SyncableEntity : AggregateRoot
{
    public Guid RoomId { get; protected set; }
    
    public long Version { get; protected set; }
    
    public DateTimeOffset UpdatedAt { get; protected set; }
    
    public bool IsDirty { get; protected set; }
    
    public bool IsDeleted { get; protected set; }

    public string? LastModifiedBy  { get; protected set; }
    public string? LastModifiedBy { get; protected set; }


}

