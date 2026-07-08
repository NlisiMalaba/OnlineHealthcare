namespace HealthPlatform.Domain.Audit;

public sealed class AuditLog
{
    private AuditLog()
    {
        Action = string.Empty;
        ResourceType = string.Empty;
    }

    public Guid Id { get; private set; }

    public Guid ActorId { get; private set; }

    public AuditActorType ActorType { get; private set; }

    public string Action { get; private set; }

    public string ResourceType { get; private set; }

    public Guid ResourceId { get; private set; }

    public string? IpAddress { get; private set; }

    public string? UserAgent { get; private set; }

    public DateTime TimestampUtc { get; private set; }

    public string? MetadataJson { get; private set; }

    public static AuditLog Create(
        Guid actorId,
        AuditActorType actorType,
        string action,
        string resourceType,
        Guid resourceId,
        DateTime timestampUtc,
        string? ipAddress,
        string? userAgent,
        string? metadataJson = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceType);

        if (actorId == Guid.Empty)
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        if (resourceId == Guid.Empty)
        {
            throw new ArgumentException("Resource id is required.", nameof(resourceId));
        }

        return new AuditLog
        {
            Id = Guid.CreateVersion7(),
            ActorId = actorId,
            ActorType = actorType,
            Action = action.Trim(),
            ResourceType = resourceType.Trim(),
            ResourceId = resourceId,
            IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim(),
            UserAgent = string.IsNullOrWhiteSpace(userAgent) ? null : userAgent.Trim(),
            TimestampUtc = timestampUtc,
            MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson
        };
    }
}
