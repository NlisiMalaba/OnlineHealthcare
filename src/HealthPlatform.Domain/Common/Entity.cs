using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Common;

public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; protected set; } = Guid.CreateVersion7();

    public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; protected set; } = DateTime.UtcNow;

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
