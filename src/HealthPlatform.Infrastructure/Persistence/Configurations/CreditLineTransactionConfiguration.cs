using HealthPlatform.Domain.Payments.CreditLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class CreditLineTransactionConfiguration : IEntityTypeConfiguration<CreditLineTransaction>
{
    public void Configure(EntityTypeBuilder<CreditLineTransaction> builder)
    {
        builder.ToTable("credit_line_transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(t => t.TransactionType)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.HasIndex(t => t.PatientId);
        builder.HasIndex(t => t.CreditLineId);
        builder.HasIndex(t => new { t.RepaymentReminderSent, t.RepaymentDueAtUtc });
    }
}
