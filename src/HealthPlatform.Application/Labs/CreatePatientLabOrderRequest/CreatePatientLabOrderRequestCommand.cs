using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Labs.CreatePatientLabOrderRequest;

public sealed record CreatePatientLabOrderRequestCommand(
    string LabPartnerCode,
    string TestCode,
    string? ClinicalNotes) : ICommand<LabOrderDto>;
