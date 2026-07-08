using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Labs.CreateDoctorLabOrder;

public sealed record CreateDoctorLabOrderCommand(
    Guid PatientId,
    Guid HealthRecordId,
    string LabPartnerCode,
    string TestCode,
    string? ClinicalNotes) : ICommand<LabOrderDto>;
