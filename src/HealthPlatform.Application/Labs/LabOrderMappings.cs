using HealthPlatform.Domain.Labs;

namespace HealthPlatform.Application.Labs;

public static class LabOrderMappings
{
    public static LabOrderDto ToDto(this LabOrder order) =>
        new(
            order.Id,
            order.PatientId,
            order.HealthRecordId,
            order.OrderingDoctorId,
            order.RequestSource,
            order.Status,
            order.LabPartnerCode,
            order.TestCode,
            order.ClinicalNotes,
            order.LabPartnerOrderReference,
            order.CreatedAtUtc,
            order.ApprovedAtUtc);
}
