using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Vaccinations;

namespace HealthPlatform.Application.Vaccinations;

public interface IVaccinationScheduleInitializer
{
    Task InitializeChildScheduleAsync(
        Guid childProfileId,
        DateOnly dateOfBirth,
        DateTime createdAtUtc,
        CancellationToken ct);

    Task InitializePatientScheduleAsync(Patient patient, DateTime createdAtUtc, CancellationToken ct);
}

public sealed class VaccinationScheduleInitializer(
    IVaccinationScheduleRepository scheduleRepository) : IVaccinationScheduleInitializer
{
    public async Task InitializeChildScheduleAsync(
        Guid childProfileId,
        DateOnly dateOfBirth,
        DateTime createdAtUtc,
        CancellationToken ct)
    {
        if (await scheduleRepository.HasScheduleForChildAsync(childProfileId, ct))
        {
            return;
        }

        var asOfDate = DateOnly.FromDateTime(createdAtUtc);
        var scheduleItems = ChildImmunizationSchedulePolicies.BuildRecommendedSchedule(dateOfBirth, asOfDate);
        if (scheduleItems.Count == 0)
        {
            return;
        }

        var entries = scheduleItems
            .Select(item => VaccinationScheduleEntry.CreateForChild(
                childProfileId,
                item.VaccineName,
                ChildImmunizationSchedulePolicies.ResolveRecommendedDate(dateOfBirth, item.DaysFromBirth),
                item.Description,
                createdAtUtc))
            .ToList();

        await scheduleRepository.AddRangeAsync(entries, ct);
    }

    public async Task InitializePatientScheduleAsync(Patient patient, DateTime createdAtUtc, CancellationToken ct)
    {
        if (await scheduleRepository.HasScheduleForPatientAsync(patient.Id, ct))
        {
            return;
        }

        var asOfDate = DateOnly.FromDateTime(createdAtUtc);
        var scheduleItems = AdultImmunizationSchedulePolicies.BuildRecommendedSchedule(
            patient.DateOfBirth,
            patient.ChronicConditions,
            asOfDate);

        if (scheduleItems.Count == 0)
        {
            return;
        }

        var entries = scheduleItems
            .Select(item => VaccinationScheduleEntry.CreateForPatient(
                patient.Id,
                item.VaccineName,
                item.RecommendedDate,
                item.Description,
                createdAtUtc))
            .ToList();

        await scheduleRepository.AddRangeAsync(entries, ct);
    }
}
