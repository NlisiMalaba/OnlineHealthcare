using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Telemedicine;
using MediatR;

namespace HealthPlatform.Application.Telemedicine.RecordingConsent;

public sealed class EnableSessionRecordingCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IAppointmentRepository appointmentRepository,
    ITelemedicineSessionRepository telemedicineSessionRepository)
    : IRequestHandler<EnableSessionRecordingCommand, EnableSessionRecordingDto>
{
    public async Task<EnableSessionRecordingDto> Handle(EnableSessionRecordingCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var doctor = await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Only the doctor can enable session recording.");

        var appointment = await appointmentRepository.GetByIdForDoctorAsync(request.AppointmentId, doctor.Id, ct)
            ?? throw new NotFoundException(
                TelemedicineErrorCodes.AppointmentNotFound,
                "Appointment was not found.");

        var session = await telemedicineSessionRepository.GetByAppointmentIdAsync(appointment.Id, ct)
            ?? throw new NotFoundException(
                TelemedicineErrorCodes.SessionNotFound,
                "Telemedicine session was not found.");

        try
        {
            session.EnableRecording();
        }
        catch (RecordingConsentRequiredException)
        {
            throw new DomainException(
                TelemedicineErrorCodes.RecordingConsentRequired,
                "Recording cannot be enabled without patient consent.");
        }
        catch (TelemedicineSessionNotJoinableException)
        {
            throw new DomainException(
                TelemedicineErrorCodes.SessionNotJoinable,
                "Telemedicine session is no longer active.");
        }

        await telemedicineSessionRepository.UpdateAsync(session, ct);

        return new EnableSessionRecordingDto(
            session.Id,
            session.AppointmentId,
            session.RecordingConsent,
            session.RecordingEnabled);
    }
}
