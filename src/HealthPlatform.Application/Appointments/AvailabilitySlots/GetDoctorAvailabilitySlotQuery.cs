using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Appointments.AvailabilitySlots;

public sealed record GetDoctorAvailabilitySlotQuery(Guid SlotId) : IQuery<DoctorAvailabilitySlotDto>;
