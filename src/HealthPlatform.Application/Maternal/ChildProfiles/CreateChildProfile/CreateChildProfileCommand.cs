using HealthPlatform.Application.Maternal.ChildProfiles;
using MediatR;

namespace HealthPlatform.Application.Maternal.ChildProfiles.CreateChildProfile;

public sealed record CreateChildProfileCommand(
    string FullName,
    DateOnly DateOfBirth,
    string? BloodType,
    IReadOnlyList<string> KnownAllergies) : IRequest<ChildProfileDto>;
