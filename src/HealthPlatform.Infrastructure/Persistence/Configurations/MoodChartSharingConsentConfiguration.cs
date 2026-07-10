using HealthPlatform.Domain.MentalHealth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class MoodChartSharingConsentConfiguration : IEntityTypeConfiguration<MoodChartSharingConsent>
{
    public void Configure(EntityTypeBuilder<MoodChartSharingConsent> builder)
    {
        builder.ToTable("mood_chart_sharing_consents");

        builder.HasKey(consent => consent.Id);

        builder.HasIndex(consent => new { consent.PatientId, consent.TherapistId });
        builder.HasIndex(consent => consent.TherapistId);
    }
}
