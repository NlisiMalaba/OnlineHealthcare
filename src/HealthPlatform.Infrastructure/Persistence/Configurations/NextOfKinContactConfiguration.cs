using HealthPlatform.Domain.NextOfKin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class NextOfKinContactConfiguration : IEntityTypeConfiguration<NextOfKinContact>
{
    public void Configure(EntityTypeBuilder<NextOfKinContact> builder)
    {
        builder.ToTable("next_of_kin_contacts");

        builder.HasKey(contact => contact.Id);

        builder.Property(contact => contact.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(contact => contact.Relationship)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(contact => contact.PhoneNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(contact => contact.Email)
            .HasMaxLength(320);

        builder.HasIndex(contact => contact.PatientId);
    }
}
