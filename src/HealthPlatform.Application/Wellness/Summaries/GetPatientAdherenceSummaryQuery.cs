using MediatR;

namespace HealthPlatform.Application.Wellness.Summaries;

public sealed record GetPatientAdherenceSummaryQuery(
    AdherenceSummaryPeriod Period) : IRequest<AdherenceSummaryDto>;
