using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Telemedicine;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Telemedicine.EventHandlers;

public sealed class CreateTelemedicineSessionOnAppointmentConfirmedNotificationHandler(
    IAppointmentRepository appointmentRepository,
    IDoctorRepository doctorRepository,
    ITelemedicineSessionRepository telemedicineSessionRepository,
    IRtcProviderResolver rtcProviderResolver,
    ILogger<CreateTelemedicineSessionOnAppointmentConfirmedNotificationHandler> logger)
    : INotificationHandler<AppointmentConfirmedNotification>
{
    public async Task Handle(AppointmentConfirmedNotification notification, CancellationToken ct)
    {
        var existing = await telemedicineSessionRepository.GetByAppointmentIdAsync(
            notification.AppointmentId,
            ct);

        if (existing is not null)
        {
            return;
        }

        var appointment = await appointmentRepository.GetByIdAsync(notification.AppointmentId, ct)
            ?? throw new NotFoundException(
                TelemedicineErrorCodes.AppointmentNotFound,
                "Appointment was not found.");

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
            logger.LogInformation(
                "Skipping telemedicine session creation for physical appointment {AppointmentId}.",
                notification.AppointmentId);

            return;
        }

        var rtcProvider = rtcProviderResolver.Resolve();
        var session = TelemedicineSession.CreateForAppointment(appointment.Id, rtcProvider);
        await telemedicineSessionRepository.AddAsync(session, ct);

        logger.LogInformation(
            "Created telemedicine session {SessionId} for appointment {AppointmentId}.",
            session.Id,
            appointment.Id);
    }
}
