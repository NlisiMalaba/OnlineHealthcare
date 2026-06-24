using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Prescriptions.CancelPrescription;

public sealed record CancelPrescriptionCommand(Guid PrescriptionId, string Reason)
    : ICommand<PrescriptionDto>;
