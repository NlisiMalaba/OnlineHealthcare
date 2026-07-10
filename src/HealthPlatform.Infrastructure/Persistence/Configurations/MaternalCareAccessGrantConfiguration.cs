using HealthPlatform.Domain.Maternal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class MaternalCareAccessGrantConfiguration : IEntityTypeConfiguration<MaternalCareAccessGrant>
{
    public void Configure(EntityTypeBuilder<MaternalCareAccessGrant> builder)
    {
        builder.ToTable("maternal_care_access_grants");

        builder.HasKey(grant => grant.Id);

        builder.HasIndex(grant => new { grant.AntenatalRecordId, grant.DoctorId, grant.RevokedAtUtc });
        builder.HasIndex(grant => grant.PatientId);
    }
}
