using HealthPlatform.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(payment => payment.Id);

        builder.Property(payment => payment.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(payment => payment.GatewayReference)
            .HasMaxLength(128);

        builder.Property(payment => payment.ReceiptStorageKey)
            .HasMaxLength(512);

        builder.Property(payment => payment.FailureCode)
            .HasMaxLength(64);

        builder.Property(payment => payment.FailureMessage)
            .HasMaxLength(500);

        builder.Property(payment => payment.PaymentMethod)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(payment => payment.Gateway)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(payment => payment.Status)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.HasIndex(payment => payment.PatientId);
        builder.HasIndex(payment => payment.AppointmentId);
        builder.HasIndex(payment => payment.MedicationOrderId);
        builder.HasIndex(payment => payment.CompletedAtUtc);
    }
}
