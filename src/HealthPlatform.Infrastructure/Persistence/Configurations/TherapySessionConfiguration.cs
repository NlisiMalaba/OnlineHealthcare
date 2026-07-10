using HealthPlatform.Domain.MentalHealth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class TherapySessionConfiguration : IEntityTypeConfiguration<TherapySession>
{
    public void Configure(EntityTypeBuilder<TherapySession> builder)
    {
        builder.ToTable("therapy_sessions");

        builder.HasKey(session => session.Id);

        builder.HasIndex(session => session.AppointmentId)
            .IsUnique();

        builder.HasIndex(session => session.PatientId);
        builder.HasIndex(session => session.TherapistId);

        builder.Property(session => session.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(session => session.SummaryRef)
            .HasMaxLength(64);

        builder.Property(session => session.SummaryEntryId)
            .HasMaxLength(64);
    }
}
