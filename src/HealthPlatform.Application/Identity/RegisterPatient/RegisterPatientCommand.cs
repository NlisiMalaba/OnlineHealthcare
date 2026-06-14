using HealthPlatform.Application.Behaviors;
using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Identity.RegisterPatient;

public sealed record RegisterPatientCommand(
    PatientAuthProvider AuthProvider,
    string FullName,
    string? PhoneNumber,
    string? Email,
    string? Password,
    string? IdToken) : ICommand<PatientRegistrationResponseDto>;
