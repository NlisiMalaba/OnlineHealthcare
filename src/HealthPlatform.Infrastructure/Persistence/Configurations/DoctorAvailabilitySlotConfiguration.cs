using HealthPlatform.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class DoctorAvailabilitySlotConfiguration : IEntityTypeConfiguration<DoctorAvailabilitySlot>
{
    public void Configure(EntityTypeBuilder<DoctorAvailabilitySlot> builder)
    {
        builder.ToTable("doctor_availability_slots");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.DayOfWeek)
            .HasConversion<int>();

        builder.Property(s => s.AppointmentType)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.HasIndex(s => s.DoctorId);
    }
}
