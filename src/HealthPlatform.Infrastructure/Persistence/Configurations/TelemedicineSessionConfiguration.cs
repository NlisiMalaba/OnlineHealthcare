using HealthPlatform.Domain.Telemedicine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class TelemedicineSessionConfiguration : IEntityTypeConfiguration<TelemedicineSession>
{
    public void Configure(EntityTypeBuilder<TelemedicineSession> builder)
    {
        builder.ToTable("telemedicine_sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ChannelName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(s => s.RtcProvider)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(s => s.Mode)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(s => s.RecordingUrl)
            .HasMaxLength(512);

        builder.Property(s => s.SessionSummaryRef)
            .HasMaxLength(128);

        builder.HasIndex(s => s.AppointmentId)
            .IsUnique();
    }
}
