using HealthPlatform.Application.Behaviors;
using HealthPlatform.Application.Identity.RegisterDoctor;

namespace HealthPlatform.Application.Identity.UpdateDoctorProfile;

public sealed record UpdateDoctorProfileCommand(
    decimal? VirtualFee,
    decimal? PhysicalFee,
    string? Bio,
    IReadOnlyList<DoctorAvailabilitySlotInput>? AvailabilitySlots,
    DoctorFileUpload? ProfilePhoto,
    DoctorFileUpload? Credentials) : ICommand<DoctorProfileDto>;
