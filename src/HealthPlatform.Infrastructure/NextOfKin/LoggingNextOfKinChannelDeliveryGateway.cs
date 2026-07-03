using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Domain.NextOfKin;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.NextOfKin;

public sealed class LoggingNextOfKinChannelDeliveryGateway(
    ILogger<LoggingNextOfKinChannelDeliveryGateway> logger) : INextOfKinChannelDeliveryGateway
{
    public Task<bool> TryDeliverEmergencyAlertAsync(
        EmergencyAlertChannelDeliveryRequest request,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Retrying emergency alert {AlertId} {Channel} delivery to next-of-kin contact {ContactId} for patient {PatientId}.",
            request.EmergencyAlertId,
            request.Channel,
            request.Contact.Id,
            request.PatientId);
        return Task.FromResult(true);
    }
}
