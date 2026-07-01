using HealthPlatform.Application.Wellness;
using HealthPlatform.Domain.Wellness;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class MedicationScheduleRepository(ApplicationDbContext db) : IMedicationScheduleRepository
{
    public Task<MedicationSchedule?> GetByPrescriptionIdAsync(Guid prescriptionId, CancellationToken ct) =>
        db.MedicationSchedules.SingleOrDefaultAsync(schedule => schedule.PrescriptionId == prescriptionId, ct);

    public Task<MedicationSchedule?> GetActiveByIdForPatientAsync(
        Guid scheduleId,
        Guid patientId,
        CancellationToken ct) =>
        db.MedicationSchedules.SingleOrDefaultAsync(
            schedule => schedule.Id == scheduleId
                && schedule.PatientId == patientId
                && schedule.Status == MedicationScheduleStatus.Active,
            ct);

    public async Task<IReadOnlyList<MedicationSchedule>> ListActiveByPatientIdAsync(
        Guid patientId,
        CancellationToken ct) =>
        await db.MedicationSchedules
            .Where(s => s.PatientId == patientId && s.Status == MedicationScheduleStatus.Active)
            .OrderBy(s => s.MedicationName)
            .ToListAsync(ct);

    public async Task AddAsync(MedicationSchedule schedule, CancellationToken ct)
    {
        await db.MedicationSchedules.AddAsync(schedule, ct);
        await db.SaveChangesAsync(ct);
    }
}
