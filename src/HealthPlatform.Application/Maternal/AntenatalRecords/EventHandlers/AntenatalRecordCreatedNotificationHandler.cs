using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Maternal.AntenatalRecords;
using HealthPlatform.Application.Maternal.AntenatalRecords.Notifications;
using MediatR;

namespace HealthPlatform.Application.Maternal.AntenatalRecords.EventHandlers;

public sealed class AntenatalRecordCreatedNotificationHandler(
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IAntenatalRecordRepository antenatalRecordRepository,
    IAntenatalRecordCreatedNotifier notifier)
    : INotificationHandler<AntenatalRecordCreatedNotification>
{
    public async Task Handle(AntenatalRecordCreatedNotification notification, CancellationToken ct)
    {
        var patient = await patientRepository.GetByIdAsync(notification.PatientId, ct)
            ?? throw new NotFoundException(
                AntenatalRecordErrorCodes.PatientNotFound,
                "Patient profile was not found.");

        var obstetricDoctor = await doctorRepository.GetByIdAsync(notification.ObstetricDoctorId, ct)
            ?? throw new NotFoundException(
                AntenatalRecordErrorCodes.DoctorNotFound,
                "Obstetric doctor profile was not found.");

        var scheduleEntries = await antenatalRecordRepository.ListScheduleEntriesByRecordIdAsync(
            notification.AntenatalRecordId,
            ct);

        await notifier.NotifyAntenatalRecordCreatedAsync(
            patient.UserId,
            obstetricDoctor.UserId,
            notification.AntenatalRecordId,
            notification.EstimatedDueDate,
            scheduleEntries.Count,
            ct);
    }
}
