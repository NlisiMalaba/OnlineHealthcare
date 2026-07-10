using HealthPlatform.Domain.Queue;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class QueueEntryConfiguration : IEntityTypeConfiguration<QueueEntry>
{
    public void Configure(EntityTypeBuilder<QueueEntry> builder)
    {
        builder.ToTable("queue_entries");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.PatientName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(entry => entry.ArrivalStatus)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(entry => entry.PositionTwoNotifiedAtUtc);

        builder.HasIndex(entry => entry.AppointmentId);
        builder.HasIndex(entry => new { entry.DoctorId, entry.ArrivalStatus });
        builder.HasIndex(entry => entry.PatientId);
    }
}
