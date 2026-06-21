using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class PharmacyConfiguration : IEntityTypeConfiguration<Pharmacy>
{
    public void Configure(EntityTypeBuilder<Pharmacy> builder)
    {
        builder.ToTable("pharmacies");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Address)
            .HasMaxLength(500)
            .IsRequired();

        builder.OwnsOne(
            p => p.Location,
            location =>
            {
                location.Property(l => l.Latitude).HasColumnName("latitude");
                location.Property(l => l.Longitude).HasColumnName("longitude");
            });

        builder.Property(p => p.ContactEmail)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(p => p.ContactPhone)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.LogoStorageKey)
            .HasMaxLength(512);

        builder.Property(p => p.VerificationStatus)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(p => p.ContactEmail)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(p => p.ContactPhone)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(p => p.UserId)
            .IsUnique();

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
