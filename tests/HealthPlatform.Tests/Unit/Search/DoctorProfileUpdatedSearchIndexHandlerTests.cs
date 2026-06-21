using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Search.EventHandlers;
using HealthPlatform.Application.Search.Notifications;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Search;

public sealed class DoctorProfileUpdatedSearchIndexHandlerTests
{
    [Fact]
    public async Task Handle_WhenDoctorIsVerified_UpsertsDoctorIndex()
    {
        var doctorId = Guid.NewGuid();
        var doctorRepository = new Mock<IDoctorRepository>();
        doctorRepository
            .Setup(repository => repository.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateVerifiedDoctor(doctorId));

        var searchService = new CapturingSearchService();
        var handler = new DoctorProfileUpdatedSearchIndexHandler(
            doctorRepository.Object,
            searchService,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<DoctorProfileUpdatedSearchIndexHandler>>());

        await handler.Handle(
            new DoctorProfileUpdatedNotification(doctorId, DateTime.UtcNow),
            CancellationToken.None);

        Assert.Single(searchService.DoctorUpserts);
        Assert.Equal(doctorId, searchService.DoctorUpserts[0]);
    }

    [Fact]
    public async Task Handle_WhenDoctorIsPending_DoesNotUpsertDoctorIndex()
    {
        var doctorId = Guid.NewGuid();
        var doctorRepository = new Mock<IDoctorRepository>();
        doctorRepository
            .Setup(repository => repository.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePendingDoctor(doctorId));

        var searchService = new CapturingSearchService();
        var handler = new DoctorProfileUpdatedSearchIndexHandler(
            doctorRepository.Object,
            searchService,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<DoctorProfileUpdatedSearchIndexHandler>>());

        await handler.Handle(
            new DoctorProfileUpdatedNotification(doctorId, DateTime.UtcNow),
            CancellationToken.None);

        Assert.Empty(searchService.DoctorUpserts);
    }

    private static Doctor CreateVerifiedDoctor(Guid doctorId)
    {
        var doctor = Doctor.Register(
            doctorId,
            Guid.NewGuid(),
            "Dr. Ada Lovelace",
            "HPCZ-12345",
            "General Practice",
            10,
            "123 Clinic Road",
            null,
            50m,
            80m,
            null,
            "ada@example.com",
            "+263771234567",
            null,
            null,
            [
                DoctorAvailabilitySlot.Create(
                    doctorId,
                    DayOfWeek.Monday,
                    new TimeOnly(9, 0),
                    new TimeOnly(12, 0),
                    30,
                    DoctorAppointmentType.Both)
            ]);

        doctor.VerifyLicense();
        return doctor;
    }

    private static Doctor CreatePendingDoctor(Guid doctorId) =>
        Doctor.Register(
            doctorId,
            Guid.NewGuid(),
            "Dr. Pending",
            "HPCZ-99999",
            "General Practice",
            5,
            "456 Clinic Road",
            null,
            40m,
            60m,
            null,
            "pending@example.com",
            "+263779999999",
            null,
            null,
            [
                DoctorAvailabilitySlot.Create(
                    doctorId,
                    DayOfWeek.Monday,
                    new TimeOnly(9, 0),
                    new TimeOnly(12, 0),
                    30,
                    DoctorAppointmentType.Both)
            ]);
}
