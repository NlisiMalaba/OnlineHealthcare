using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using MediatR;

namespace HealthPlatform.Application.Appointments.AvailabilitySlots;

public sealed class ListDoctorAvailabilitySlotsQueryHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository)
    : IRequestHandler<ListDoctorAvailabilitySlotsQuery, IReadOnlyList<DoctorAvailabilitySlotDto>>
{
    public async Task<IReadOnlyList<DoctorAvailabilitySlotDto>> Handle(
        ListDoctorAvailabilitySlotsQuery request,
        CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var doctor = await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new NotFoundException(
                AppointmentErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");

        return doctor.AvailabilitySlots
            .OrderBy(slot => slot.DayOfWeek)
            .ThenBy(slot => slot.StartTime)
            .Select(slot => slot.ToDto())
            .ToList();
    }
}
