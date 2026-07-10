using HealthPlatform.Application.Maternal.BirthPlans;
using MediatR;

namespace HealthPlatform.Application.Maternal.BirthPlans.ListMaternalCareAccessGrants;

public sealed record ListMaternalCareAccessGrantsQuery(Guid AntenatalRecordId)
    : IRequest<IReadOnlyList<MaternalCareAccessGrantDto>>;
