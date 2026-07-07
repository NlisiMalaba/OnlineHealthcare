using HealthPlatform.Domain.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(log => log.Id);

        builder.Property(log => log.Action)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(log => log.ResourceType)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(log => log.IpAddress)
            .HasMaxLength(64);

        builder.Property(log => log.UserAgent)
            .HasMaxLength(512);

        builder.Property(log => log.MetadataJson)
            .HasColumnType("text");

        builder.Property(log => log.ActorType)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.HasIndex(log => log.ResourceType);
        builder.HasIndex(log => log.TimestampUtc);
        builder.HasIndex(log => new { log.ActorId, log.TimestampUtc });
    }
}
