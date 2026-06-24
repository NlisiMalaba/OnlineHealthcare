using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Application.Telemedicine.Realtime.Reconnection;
using HealthPlatform.Domain.Telemedicine;
using MediatR;

namespace HealthPlatform.Application.Telemedicine.JoinSession;

public sealed class JoinTelemedicineSessionCommandHandler(
    TimeProvider timeProvider,
    ISender sender,
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IAppointmentRepository appointmentRepository,
    ITelemedicineSessionRepository telemedicineSessionRepository,
    IRtcTokenService rtcTokenService)
    : IRequestHandler<JoinTelemedicineSessionCommand, JoinTelemedicineSessionDto>
{
    public async Task<JoinTelemedicineSessionDto> Handle(JoinTelemedicineSessionCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var appointment = await appointmentRepository.GetByIdAsync(request.AppointmentId, ct)
            ?? throw new NotFoundException(
                TelemedicineErrorCodes.AppointmentNotFound,
                "Appointment was not found.");

        if (appointment.Status != AppointmentStatus.Confirmed)
        {
            throw new DomainException(
                TelemedicineErrorCodes.AppointmentNotConfirmed,
                "Only confirmed appointments can join a telemedicine session.");
        }

        var participantUid = await ResolveParticipantUidAsync(userId, appointment, ct);

        var doctor = await doctorRepository.GetByIdWithSlotsAsync(appointment.DoctorId, ct)
            ?? throw new NotFoundException(
                TelemedicineErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");

        var slot = doctor.AvailabilitySlots.SingleOrDefault(s => s.Id == appointment.SlotId && s.IsActive);
        if (slot is null)
        {
            throw new NotFoundException(
                TelemedicineErrorCodes.AvailabilitySlotNotFound,
                "Availability slot was not found.");
        }

        if (!VirtualAppointmentRules.SupportsTelemedicine(slot.AppointmentType))
        {
            throw new DomainException(
                TelemedicineErrorCodes.NotVirtualAppointment,
                "This appointment does not support telemedicine.");
        }

        var session = await telemedicineSessionRepository.GetByAppointmentIdAsync(appointment.Id, ct)
            ?? throw new NotFoundException(
                TelemedicineErrorCodes.SessionNotFound,
                "Telemedicine session was not found.");

        try
        {
            session.MarkJoined(timeProvider.GetUtcNow().UtcDateTime, request.Mode);
        }
        catch (TelemedicineSessionNotJoinableException)
        {
            throw new DomainException(
                TelemedicineErrorCodes.SessionNotJoinable,
                "Telemedicine session is no longer joinable.");
        }

        await telemedicineSessionRepository.UpdateAsync(session, ct);

        await sender.Send(new CompleteTelemedicineReconnectionCommand(request.AppointmentId), ct);

        var tokenResult = await rtcTokenService.GenerateTokenAsync(
            new RtcTokenRequest(
                session.ChannelName,
                participantUid,
                session.RtcProvider,
                TelemedicinePolicies.RtcTokenTtl),
            ct);

        return new JoinTelemedicineSessionDto(
            session.Id,
            session.AppointmentId,
            session.ChannelName,
            tokenResult.Token,
            session.RtcProvider,
            participantUid,
            session.Mode,
            tokenResult.ExpiresAtUtc);
    }

    private async Task<uint> ResolveParticipantUidAsync(
        Guid userId,
        Appointment appointment,
        CancellationToken ct)
    {
        var patient = await patientRepository.GetByUserIdAsync(userId, ct);
        if (patient is not null && patient.Id == appointment.PatientId)
        {
            return TelemedicineUid.ForPatient(patient.Id);
        }

        var doctor = await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct);
        if (doctor is not null && doctor.Id == appointment.DoctorId)
        {
            return TelemedicineUid.ForDoctor(doctor.Id);
        }

        throw new AccessDeniedException("ACCESS_DENIED", "Only the appointment patient or doctor can join.");
    }
}
