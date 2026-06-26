using HealthPlatform.Application.PharmacyOrders;
using HealthPlatform.Domain.Pharmacy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.PharmacyServices;

public sealed class DeliveryAgentAssignmentOptions
{
    public const string SectionName = "Pharmacy:Delivery";

    public string TrackingBaseUrl { get; set; } = "https://track.healthplatform.local";

    public string AgentNamePrefix { get; set; } = "Courier";
}

public sealed class ConfigurableDeliveryAgentAssignmentService(
    IOptions<DeliveryAgentAssignmentOptions> options,
    ILogger<ConfigurableDeliveryAgentAssignmentService> logger)
    : IDeliveryAgentAssignmentService
{
    public Task<DeliveryAssignmentDto> AssignForOrderAsync(Guid orderId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var settings = options.Value;
        var agentName = $"{settings.AgentNamePrefix} {orderId.ToString("N")[..8].ToUpperInvariant()}";
        var trackingPath = string.Format(MedicationOrderPolicies.TrackingUrlPathTemplate, orderId);
        var trackingUrl = $"{settings.TrackingBaseUrl.TrimEnd('/')}{trackingPath}";

        logger.LogInformation(
            "Assigned delivery agent for medication order {OrderId} with tracking URL.",
            orderId);

        return Task.FromResult(new DeliveryAssignmentDto(agentName, trackingUrl));
    }
}
