using HealthPlatform.Application.Maternal.BirthPlans;
using MediatR;

namespace HealthPlatform.Application.Maternal.BirthPlans.RevokeMaternalCareAccess;

public sealed record RevokeMaternalCareAccessCommand(
    Guid AntenatalRecordId,
    Guid DoctorId) : IRequest<MaternalCareAccessGrantDto>;
