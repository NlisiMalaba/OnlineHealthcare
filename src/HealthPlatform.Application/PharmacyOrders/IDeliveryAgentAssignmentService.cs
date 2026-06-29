namespace HealthPlatform.Application.PharmacyOrders;

public sealed record DeliveryAssignmentDto(string AgentName, string TrackingUrl);

public interface IDeliveryAgentAssignmentService
{
    Task<DeliveryAssignmentDto> AssignForOrderAsync(Guid orderId, CancellationToken ct);
}
