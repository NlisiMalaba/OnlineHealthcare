using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using MediatR;

namespace HealthPlatform.Application.Appointments.AvailabilitySlots;

public sealed class GetDoctorAvailabilitySlotQueryHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository)
    : IRequestHandler<GetDoctorAvailabilitySlotQuery, DoctorAvailabilitySlotDto>
{
    public async Task<DoctorAvailabilitySlotDto> Handle(GetDoctorAvailabilitySlotQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var doctor = await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new NotFoundException(
                AppointmentErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");

        var slot = doctor.AvailabilitySlots.SingleOrDefault(s => s.Id == request.SlotId)
            ?? throw new NotFoundException(
                AppointmentErrorCodes.AvailabilitySlotNotFound,
                "Availability slot was not found.");

        return slot.ToDto();
    }
}
