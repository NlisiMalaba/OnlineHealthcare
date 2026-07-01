using MediatR;

namespace HealthPlatform.Application.Wellness.Summaries;

public sealed record GetDoctorPatientAdherenceSummaryQuery(
    Guid PatientId,
    AdherenceSummaryPeriod Period) : IRequest<AdherenceSummaryDto>;
