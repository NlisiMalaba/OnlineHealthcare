using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Telemedicine;

namespace HealthPlatform.Application.Telemedicine.Realtime;

public sealed class TelemedicineSessionParticipantService(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IAppointmentRepository appointmentRepository,
    ITelemedicineSessionRepository telemedicineSessionRepository)
    : ITelemedicineSessionParticipantService
{
    public async Task<TelemedicineSessionParticipantContext> ResolveParticipantAsync(
        Guid appointmentId,
        bool requireActiveSession,
        CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var appointment = await appointmentRepository.GetByIdAsync(appointmentId, ct)
            ?? throw new NotFoundException(
                TelemedicineErrorCodes.AppointmentNotFound,
                "Appointment was not found.");

        if (appointment.Status != AppointmentStatus.Confirmed)
        {
            throw new DomainException(
                TelemedicineErrorCodes.AppointmentNotConfirmed,
                "Only confirmed appointments can access telemedicine sessions.");
        }

        var role = await ResolveRoleAsync(userId, appointment, ct);

        var session = await telemedicineSessionRepository.GetByAppointmentIdAsync(appointment.Id, ct)
            ?? throw new NotFoundException(
                TelemedicineErrorCodes.SessionNotFound,
                "Telemedicine session was not found.");

        if (session.Status is TelemedicineSessionStatus.Ended or TelemedicineSessionStatus.Interrupted)
        {
            throw new DomainException(
                TelemedicineErrorCodes.SessionNotJoinable,
                "Telemedicine session is no longer available.");
        }

        if (requireActiveSession && session.Status != TelemedicineSessionStatus.Active)
        {
            throw new DomainException(
                TelemedicineErrorCodes.SessionNotActive,
                "Telemedicine session is not active.");
        }

        return new TelemedicineSessionParticipantContext(
            appointment.Id,
            session.Id,
            role,
            session);
    }

    private async Task<TelemedicineSessionParticipantRole> ResolveRoleAsync(
        Guid userId,
        Appointment appointment,
        CancellationToken ct)
    {
        var patient = await patientRepository.GetByUserIdAsync(userId, ct);
        if (patient is not null && patient.Id == appointment.PatientId)
        {
            return TelemedicineSessionParticipantRole.Patient;
        }

        var doctor = await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct);
        if (doctor is not null && doctor.Id == appointment.DoctorId)
        {
            return TelemedicineSessionParticipantRole.Doctor;
        }

        throw new AccessDeniedException(
            "ACCESS_DENIED",
            "Only the appointment patient or doctor can access this session.");
    }
}
