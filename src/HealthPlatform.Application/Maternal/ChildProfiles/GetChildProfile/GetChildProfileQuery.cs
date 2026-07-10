using HealthPlatform.Application.Maternal.ChildProfiles;
using MediatR;

namespace HealthPlatform.Application.Maternal.ChildProfiles.GetChildProfile;

public sealed record GetChildProfileQuery(Guid ChildProfileId) : IRequest<ChildProfileDto>;
