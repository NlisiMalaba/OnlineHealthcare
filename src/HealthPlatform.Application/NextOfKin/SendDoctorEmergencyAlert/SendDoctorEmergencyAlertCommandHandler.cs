using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.NextOfKin;
using MediatR;

namespace HealthPlatform.Application.NextOfKin.SendDoctorEmergencyAlert;

public sealed class SendDoctorEmergencyAlertCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IAppointmentRepository appointmentRepository,
    IEmergencyAlertDispatchService emergencyAlertDispatchService)
    : IRequestHandler<SendDoctorEmergencyAlertCommand, EmergencyAlertDto>
{
    public async Task<EmergencyAlertDto> Handle(SendDoctorEmergencyAlertCommand request, CancellationToken ct)
    {
        var doctor = await ResolveVerifiedDoctorAsync(ct);
        await EnsureAppointmentEligibleAsync(request.AppointmentId, doctor.Id, request.PatientId, ct);

        return await emergencyAlertDispatchService.DispatchAsync(
            new EmergencyAlertDispatchRequest(
                request.PatientId,
                EmergencyAlertTriggerSource.Doctor,
                request.TriggerReason,
                doctor.Id,
                request.AppointmentId),
            ct);
    }

    private async Task<Doctor> ResolveVerifiedDoctorAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var doctor = await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new NotFoundException(
                NextOfKinErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");

        if (doctor.VerificationStatus != DoctorVerificationStatus.Verified)
        {
            throw new DomainException(
                NextOfKinErrorCodes.DoctorNotVerified,
                "Only verified doctors can send emergency alerts.");
        }

        return doctor;
    }

    private async Task EnsureAppointmentEligibleAsync(
        Guid appointmentId,
        Guid doctorId,
        Guid patientId,
        CancellationToken ct)
    {
        var appointment = await appointmentRepository.GetByIdForDoctorAsync(appointmentId, doctorId, ct)
            ?? throw new NotFoundException(
                NextOfKinErrorCodes.AppointmentNotFound,
                "Appointment was not found.");

        if (appointment.PatientId != patientId)
        {
            throw new AccessDeniedException(
                "ACCESS_DENIED",
                "Appointment does not belong to the specified patient.");
        }

        if (appointment.Status != AppointmentStatus.Confirmed)
        {
            throw new DomainException(
                NextOfKinErrorCodes.AppointmentNotEligibleForEmergencyAlert,
                "Emergency alerts can only be sent during a confirmed consultation.");
        }
    }
}
