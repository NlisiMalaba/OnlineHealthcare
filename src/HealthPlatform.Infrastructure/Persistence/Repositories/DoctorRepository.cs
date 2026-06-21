using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class DoctorRepository(ApplicationDbContext db) : IDoctorRepository
{
    public Task<bool> ExistsByLicenseNumberAsync(string licenseNumber, CancellationToken ct) =>
        db.Doctors.AnyAsync(
            d => d.LicenseNumber == licenseNumber.Trim().ToUpperInvariant(),
            ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct) =>
        db.Doctors.AnyAsync(d => d.Email == email.Trim().ToLowerInvariant(), ct);

    public Task<bool> ExistsByPhoneAsync(string phoneNumber, CancellationToken ct) =>
        db.Doctors.AnyAsync(d => d.PhoneNumber == phoneNumber.Trim(), ct);

    public async Task AddAsync(Doctor doctor, CancellationToken ct)
    {
        await db.Doctors.AddAsync(doctor, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task<Doctor?> GetByIdAsync(Guid doctorId, CancellationToken ct) =>
        db.Doctors.SingleOrDefaultAsync(d => d.Id == doctorId, ct);

    public Task<Doctor?> GetByIdWithSlotsAsync(Guid doctorId, CancellationToken ct) =>
        db.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleOrDefaultAsync(d => d.Id == doctorId, ct);

    public Task<Doctor?> GetByUserIdWithSlotsAsync(Guid userId, CancellationToken ct) =>
        db.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleOrDefaultAsync(d => d.UserId == userId, ct);

    public Task UpdateAsync(Doctor doctor, CancellationToken ct) =>
        db.SaveChangesAsync(ct);

    public async Task ReplaceAvailabilitySlotsAsync(
        Guid doctorId,
        IReadOnlyList<DoctorAvailabilitySlot> slots,
        CancellationToken ct)
    {
        var existing = await db.DoctorAvailabilitySlots
            .Where(s => s.DoctorId == doctorId)
            .ToListAsync(ct);

        if (existing.Count > 0)
        {
            db.DoctorAvailabilitySlots.RemoveRange(existing);
        }

        await db.DoctorAvailabilitySlots.AddRangeAsync(slots, ct);
        await db.SaveChangesAsync(ct);
    }
}
