namespace HealthPlatform.API.Requests.Queue;

public sealed class JoinQueueRequest
{
    public Guid AppointmentId { get; init; }
}
