using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Appointments.EventHandlers;
using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Identity;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Appointments;

public sealed class AppointmentConfirmedNotificationHandlerTests
{
    [Fact]
    public async Task Handle_Sends_confirmation_to_patient_and_doctor()
    {
        var patient = Patient.RegisterWithEmail(
            Guid.CreateVersion7(),
            "Patient One",
            "patient1@example.com");

        var doctorId = Guid.CreateVersion7();
        var doctor = Doctor.Register(
            doctorId,
            Guid.CreateVersion7(),
            "Dr. One",
            "HPCZ-00001",
            "General Practice",
            10,
            "10 Clinic Street",
            null,
            10m,
            20m,
            null,
            "doctor1@example.com",
            "+263771111111",
            null,
            null,
            [
                DoctorAvailabilitySlot.Create(
                    doctorId,
                    DayOfWeek.Monday,
                    new TimeOnly(9, 0),
                    new TimeOnly(10, 0),
                    30,
                    DoctorAppointmentType.Virtual)
            ]);

        var patientRepository = new Mock<IPatientRepository>();
        patientRepository
            .Setup(repo => repo.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var doctorRepository = new Mock<IDoctorRepository>();
        doctorRepository
            .Setup(repo => repo.GetByIdAsync(doctor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        var notifier = new Mock<IAppointmentConfirmationNotifier>();
        var handler = new AppointmentConfirmedNotificationHandler(
            patientRepository.Object,
            doctorRepository.Object,
            notifier.Object);

        var notification = new AppointmentConfirmedNotification(
            Guid.CreateVersion7(),
            patient.Id,
            doctor.Id,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow,
            DateTime.UtcNow);

        await handler.Handle(notification, CancellationToken.None);

        notifier.Verify(x => x.NotifyAppointmentConfirmedAsync(
            patient.UserId,
            doctor.UserId,
            notification.AppointmentId,
            notification.ScheduledAtUtc,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
