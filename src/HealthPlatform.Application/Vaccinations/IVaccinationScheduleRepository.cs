using HealthPlatform.Domain.Vaccinations;

namespace HealthPlatform.Application.Vaccinations;

public interface IVaccinationScheduleRepository
{
    Task AddRangeAsync(IReadOnlyCollection<VaccinationScheduleEntry> entries, CancellationToken ct);

    Task<bool> HasScheduleForChildAsync(Guid childProfileId, CancellationToken ct);

    Task<bool> HasScheduleForPatientAsync(Guid patientId, CancellationToken ct);

    Task<IReadOnlyList<VaccinationScheduleEntry>> ListByChildProfileIdAsync(Guid childProfileId, CancellationToken ct);

    Task<IReadOnlyList<VaccinationScheduleEntry>> ListByPatientIdAsync(Guid patientId, CancellationToken ct);

    Task<VaccinationScheduleEntry?> GetByIdAsync(Guid scheduleEntryId, CancellationToken ct);

    Task<IReadOnlyList<VaccinationScheduleEntry>> ListDueForReminderAsync(DateOnly asOfDate, CancellationToken ct);

    Task UpdateAsync(VaccinationScheduleEntry entry, CancellationToken ct);
}
