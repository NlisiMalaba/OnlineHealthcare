using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Appointments.AvailabilitySlots;

public sealed record ListDoctorAvailabilitySlotsQuery : IQuery<IReadOnlyList<DoctorAvailabilitySlotDto>>;
