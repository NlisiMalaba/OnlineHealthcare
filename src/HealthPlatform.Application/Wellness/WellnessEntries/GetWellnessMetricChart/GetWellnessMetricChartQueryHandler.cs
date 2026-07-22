using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.Wellness.WellnessEntries.GetWellnessMetricChart;

public sealed class GetWellnessMetricChartQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IWellnessEntryRepository wellnessEntryRepository,
    TimeProvider timeProvider)
    : IRequestHandler<GetWellnessMetricChartQuery, WellnessMetricChartDto>
{
    public async Task<WellnessMetricChartDto> Handle(GetWellnessMetricChartQuery request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var toUtc = NormalizeUtc(request.ToUtc ?? timeProvider.GetUtcNow().UtcDateTime);
        var fromUtc = NormalizeUtc(
            request.FromUtc ?? toUtc.Subtract(WellnessPolicies.WellnessChartDefaultLookback));

        var entries = await wellnessEntryRepository.ListByPatientIdAsync(
            patient.Id,
            request.MetricType,
            fromUtc,
            toUtc,
            ct);

        return new WellnessMetricChartDto(
            request.MetricType,
            fromUtc,
            toUtc,
            entries
                .OrderBy(entry => entry.RecordedAtUtc)
                .Select(entry => new WellnessMetricChartPointDto(entry.RecordedAtUtc, entry.Value))
                .ToList());
    }

    private async Task<Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                WellnessErrorCodes.PatientNotFound,
                "Patient profile was not found.");
    }

    private static DateTime NormalizeUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
