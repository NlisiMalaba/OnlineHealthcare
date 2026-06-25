using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Telemedicine;
using MediatR;

namespace HealthPlatform.Application.Telemedicine.RecordingConsent;

public sealed class GrantRecordingConsentCommandHandler(
    TimeProvider timeProvider,
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IAppointmentRepository appointmentRepository,
    ITelemedicineSessionRepository telemedicineSessionRepository)
    : IRequestHandler<GrantRecordingConsentCommand, GrantRecordingConsentDto>
{
    public async Task<GrantRecordingConsentDto> Handle(GrantRecordingConsentCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var patient = await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Only the patient can grant recording consent.");

        var appointment = await appointmentRepository.GetByIdForPatientAsync(request.AppointmentId, patient.Id, ct)
            ?? throw new NotFoundException(
                TelemedicineErrorCodes.AppointmentNotFound,
                "Appointment was not found.");

        var session = await telemedicineSessionRepository.GetByAppointmentIdAsync(appointment.Id, ct)
            ?? throw new NotFoundException(
                TelemedicineErrorCodes.SessionNotFound,
                "Telemedicine session was not found.");

        try
        {
            session.GrantRecordingConsent(timeProvider.GetUtcNow().UtcDateTime);
        }
        catch (RecordingConsentNotAllowedException)
        {
            throw new DomainException(
                TelemedicineErrorCodes.RecordingConsentNotAllowed,
                "Recording consent must be granted before the session begins.");
        }

        await telemedicineSessionRepository.UpdateAsync(session, ct);

        return new GrantRecordingConsentDto(
            session.Id,
            session.AppointmentId,
            session.RecordingConsent,
            session.RecordingEnabled);
    }
}
