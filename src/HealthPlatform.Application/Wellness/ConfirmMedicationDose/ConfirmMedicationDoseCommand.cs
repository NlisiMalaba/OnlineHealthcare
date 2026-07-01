using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Wellness.ConfirmMedicationDose;

public sealed record ConfirmMedicationDoseCommand(
    Guid ScheduleId,
    DateTime ScheduledAtUtc) : ICommand<AdherenceEventDto>;
