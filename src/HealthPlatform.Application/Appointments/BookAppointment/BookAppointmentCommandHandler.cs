using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Payments;
using MediatR;

namespace HealthPlatform.Application.Appointments.BookAppointment;

public sealed class BookAppointmentCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IAppointmentRepository appointmentRepository,
    ISlotHoldService slotHoldService)
    : IRequestHandler<BookAppointmentCommand, BookAppointmentDto>
{
    private static readonly TimeSpan SlotHoldTtl = PaymentPolicies.PendingRetentionWindow;

    public async Task<BookAppointmentDto> Handle(BookAppointmentCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var patient = await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                AppointmentErrorCodes.PatientNotFound,
                "Patient profile was not found.");

        var doctor = await doctorRepository.GetByIdWithSlotsAsync(request.DoctorId, ct)
            ?? throw new NotFoundException(
                AppointmentErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");

        var slot = doctor.AvailabilitySlots.SingleOrDefault(s => s.Id == request.SlotId && s.IsActive);
        if (slot is null)
        {
            throw new NotFoundException(
                AppointmentErrorCodes.AvailabilitySlotNotFound,
                "Availability slot was not found.");
        }

        var acquired = await slotHoldService.TryHoldAsync(request.SlotId, patient.Id, SlotHoldTtl, ct);
        if (!acquired)
        {
            throw new ConflictException(
                AppointmentErrorCodes.SlotUnavailable,
                "The selected slot is currently unavailable.");
        }

        var holdExpiresAtUtc = DateTime.UtcNow.Add(SlotHoldTtl);
        var appointment = Appointment.CreatePendingPayment(
            patient.Id,
            doctor.Id,
            request.SlotId,
            request.ScheduledAtUtc,
            holdExpiresAtUtc);

        await appointmentRepository.AddAsync(appointment, ct);

        return new BookAppointmentDto(
            appointment.Id,
            appointment.DoctorId,
            appointment.SlotId,
            appointment.ScheduledAtUtc,
            "pending_payment",
            appointment.SlotHoldExpiresAtUtc,
            slot.AppointmentType,
            AppointmentClinicMappings.ToClinicDto(doctor, slot.AppointmentType));
    }
}
