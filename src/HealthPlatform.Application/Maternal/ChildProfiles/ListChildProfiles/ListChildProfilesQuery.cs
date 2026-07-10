using HealthPlatform.Application.Maternal.ChildProfiles;
using MediatR;

namespace HealthPlatform.Application.Maternal.ChildProfiles.ListChildProfiles;

public sealed record ListChildProfilesQuery : IRequest<IReadOnlyList<ChildProfileDto>>;
