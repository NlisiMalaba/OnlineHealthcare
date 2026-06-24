using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Prescriptions.Dispensing;

public sealed record DispensePrescriptionForMedicationOrderCommand(Guid PrescriptionId)
    : ICommand<PrescriptionDto>;
