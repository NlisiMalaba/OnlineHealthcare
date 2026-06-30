using HealthPlatform.Domain.Payments.CreditLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class PatientCreditLineConfiguration : IEntityTypeConfiguration<PatientCreditLine>
{
    public void Configure(EntityTypeBuilder<PatientCreditLine> builder)
    {
        builder.ToTable("patient_credit_lines");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(c => c.CreditScore)
            .HasPrecision(9, 2);

        builder.HasIndex(c => c.PatientId).IsUnique();
    }
}
