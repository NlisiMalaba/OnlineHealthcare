using HealthPlatform.Application.Wellness.CarePlans;
using HealthPlatform.Domain.Wellness;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class CarePlanRepository(ApplicationDbContext db) : ICarePlanRepository
{
    public async Task<CarePlan?> GetByIdAsync(Guid carePlanId, CancellationToken ct) =>
        await db.CarePlans.FirstOrDefaultAsync(plan => plan.Id == carePlanId, ct);

    public async Task<CarePlan?> GetByIdForPatientAsync(Guid carePlanId, Guid patientId, CancellationToken ct) =>
        await db.CarePlans.FirstOrDefaultAsync(
            plan => plan.Id == carePlanId && plan.PatientId == patientId,
            ct);

    public async Task<CarePlan?> GetByIdForDoctorAsync(Guid carePlanId, Guid doctorId, CancellationToken ct) =>
        await db.CarePlans.FirstOrDefaultAsync(
            plan => plan.Id == carePlanId && plan.DoctorId == doctorId,
            ct);

    public async Task<IReadOnlyList<CarePlan>> ListByPatientIdAsync(
        Guid patientId,
        CarePlanStatus? status,
        CancellationToken ct)
    {
        var query = db.CarePlans
            .AsNoTracking()
            .Where(plan => plan.PatientId == patientId);

        if (status.HasValue)
        {
            query = query.Where(plan => plan.Status == status.Value);
        }

        return await query
            .OrderByDescending(plan => plan.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CarePlan>> ListByDoctorIdAsync(
        Guid doctorId,
        CarePlanStatus? status,
        CancellationToken ct)
    {
        var query = db.CarePlans
            .AsNoTracking()
            .Where(plan => plan.DoctorId == doctorId);

        if (status.HasValue)
        {
            query = query.Where(plan => plan.Status == status.Value);
        }

        return await query
            .OrderByDescending(plan => plan.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CarePlan>> ListActiveForTaskRemindersAsync(int take, CancellationToken ct) =>
        await db.CarePlans
            .Where(plan => plan.Status == CarePlanStatus.Active)
            .OrderBy(plan => plan.CreatedAtUtc)
            .Take(take)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CarePlan>> ListDueForReviewReminderAsync(
        DateOnly asOfDate,
        int take,
        CancellationToken ct) =>
        await db.CarePlans
            .Where(plan =>
                plan.Status == CarePlanStatus.Active
                && plan.NextReviewAt <= asOfDate
                && plan.ReviewReminderSentAtUtc == null)
            .OrderBy(plan => plan.NextReviewAt)
            .Take(take)
            .ToListAsync(ct);

    public async Task AddAsync(CarePlan carePlan, CancellationToken ct)
    {
        await db.CarePlans.AddAsync(carePlan, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(CarePlan carePlan, CancellationToken ct)
    {
        db.CarePlans.Update(carePlan);
        await db.SaveChangesAsync(ct);
    }
}
