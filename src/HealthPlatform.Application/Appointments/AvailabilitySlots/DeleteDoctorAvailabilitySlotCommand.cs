using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Appointments.AvailabilitySlots;

public sealed record DeleteDoctorAvailabilitySlotCommand(Guid SlotId) : ICommand;
