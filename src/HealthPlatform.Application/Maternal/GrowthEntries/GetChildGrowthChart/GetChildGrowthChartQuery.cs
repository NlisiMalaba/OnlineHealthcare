using HealthPlatform.Application.Maternal.GrowthEntries;
using MediatR;

namespace HealthPlatform.Application.Maternal.GrowthEntries.GetChildGrowthChart;

public sealed record GetChildGrowthChartQuery(Guid ChildProfileId) : IRequest<GrowthChartDto>;
