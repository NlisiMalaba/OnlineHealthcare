using HealthPlatform.Application.Behaviors;
using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Identity.RegisterDoctor;

public sealed record DoctorFileUpload(Stream Content, string ContentType, string FileName, long Length)
    : IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        await Content.DisposeAsync();
    }
}

public sealed record DoctorAvailabilitySlotInput(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int SlotDurationMinutes,
    DoctorAppointmentType AppointmentType);

public sealed record RegisterDoctorCommand(
    string FullName,
    string LicenseNumber,
    string Specialty,
    int YearsOfExperience,
    string ClinicAddress,
    double? ClinicLatitude,
    double? ClinicLongitude,
    decimal VirtualFee,
    decimal PhysicalFee,
    string? Bio,
    string Email,
    string PhoneNumber,
    string Password,
    IReadOnlyList<DoctorAvailabilitySlotInput> AvailabilitySlots,
    DoctorFileUpload? ProfilePhoto,
    DoctorFileUpload? Credentials) : ICommand<DoctorRegistrationResponseDto>;
