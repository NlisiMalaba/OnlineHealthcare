using HealthPlatform.Domain.Payments.Instalments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class InstalmentPlanConfiguration : IEntityTypeConfiguration<InstalmentPlan>
{
    public void Configure(EntityTypeBuilder<InstalmentPlan> builder)
    {
        builder.ToTable("instalment_plans");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(p => p.Frequency)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.HasIndex(p => p.PatientId);
    }
}
