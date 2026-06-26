using HealthPlatform.Domain.Pharmacy;

namespace HealthPlatform.Application.PharmacyOrders;

internal static class MedicationOrderMappings
{
    public static MedicationOrderDto ToDto(this MedicationOrder order) =>
        new(
            order.Id,
            order.PatientId,
            order.PharmacyId,
            order.PrescriptionId,
            order.MedicationSku,
            order.MedicationName,
            order.Dosage,
            order.Frequency,
            order.DurationDays,
            order.SpecialInstructions,
            order.DeliveryType.ToString().ToLowerInvariant(),
            order.DeliveryAddress,
            order.Status.ToString().ToLowerInvariant(),
            order.DeliveryAgentName,
            order.TrackingUrl,
            order.RejectionReason,
            order.ClarificationMessage,
            order.CreatedAtUtc);
}
