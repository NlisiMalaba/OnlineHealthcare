using HealthPlatform.Domain.Payments.Instalments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class InstalmentPaymentConfiguration : IEntityTypeConfiguration<InstalmentPayment>
{
    public void Configure(EntityTypeBuilder<InstalmentPayment> builder)
    {
        builder.ToTable("instalment_payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.HasIndex(p => p.InstalmentPlanId);
        builder.HasIndex(p => p.PatientId);
        builder.HasIndex(p => new { p.Status, p.DueReminderSent, p.DueDate });
        builder.HasIndex(p => new { p.InstalmentPlanId, p.SequenceNumber }).IsUnique();
    }
}
