using HealthPlatform.Application.Maternal.BirthPlans;
using MediatR;

namespace HealthPlatform.Application.Maternal.BirthPlans.GrantMaternalCareAccess;

public sealed record GrantMaternalCareAccessCommand(
    Guid AntenatalRecordId,
    Guid DoctorId,
    bool ShareAntenatalRecord,
    bool ShareBirthPlan) : IRequest<MaternalCareAccessGrantDto>;
