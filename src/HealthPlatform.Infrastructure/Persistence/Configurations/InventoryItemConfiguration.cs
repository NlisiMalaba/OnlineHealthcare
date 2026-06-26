using HealthPlatform.Domain.Pharmacy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("inventory_items");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.MedicationName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(item => item.MedicationSku)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(item => new { item.PharmacyId, item.MedicationSku }).IsUnique();
        builder.HasIndex(item => item.PharmacyId);
    }
}
