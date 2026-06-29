using HealthPlatform.Application.Behaviors;
using HealthPlatform.Domain.Insurance;

namespace HealthPlatform.Application.Insurance.SubmitInsuranceClaim;

public sealed record SubmitInsuranceClaimCommand(
    string InsurerCode,
    InsuranceClaimType ClaimType,
    long AmountMinorUnits,
    string Currency,
    Guid? AppointmentId,
    Guid? MedicationOrderId,
    Guid? LabOrderId) : ICommand<InsuranceClaimDto>;
