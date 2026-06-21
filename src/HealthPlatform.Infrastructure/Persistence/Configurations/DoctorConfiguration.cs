using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("doctors");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(d => d.LicenseNumber)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(d => d.Specialty)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(d => d.ClinicAddress)
            .HasMaxLength(500)
            .IsRequired();

        builder.OwnsOne(
            d => d.ClinicLocation,
            location =>
            {
                location.Property(p => p.Latitude).HasColumnName("clinic_latitude");
                location.Property(p => p.Longitude).HasColumnName("clinic_longitude");
            });

        builder.Property(d => d.VirtualFee)
            .HasPrecision(18, 2);

        builder.Property(d => d.PhysicalFee)
            .HasPrecision(18, 2);

        builder.Property(d => d.Bio)
            .HasMaxLength(2000);

        builder.Property(d => d.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(d => d.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(d => d.ProfilePhotoStorageKey)
            .HasMaxLength(512);

        builder.Property(d => d.CredentialsStorageKey)
            .HasMaxLength(512);

        builder.Property(d => d.VerificationStatus)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(d => d.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(d => d.AverageRating)
            .HasPrecision(4, 2);

        builder.HasMany(d => d.AvailabilitySlots)
            .WithOne()
            .HasForeignKey(s => s.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(d => d.AvailabilitySlots)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_availabilitySlots");

        builder.HasIndex(d => d.LicenseNumber)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(d => d.Email)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(d => d.PhoneNumber)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(d => d.UserId)
            .IsUnique();

        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
