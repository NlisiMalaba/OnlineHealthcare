using HealthPlatform.Domain.Insurance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class PatientInsurancePolicyConfiguration : IEntityTypeConfiguration<PatientInsurancePolicy>
{
    public void Configure(EntityTypeBuilder<PatientInsurancePolicy> builder)
    {
        builder.ToTable("patient_insurance_policies");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.InsurerCode)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(p => p.PolicyNumber)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(p => p.MemberNumber)
            .HasMaxLength(64);

        builder.HasIndex(p => new { p.PatientId, p.InsurerCode, p.PolicyNumber }).IsUnique();
        builder.HasIndex(p => p.PatientId);
    }
}
