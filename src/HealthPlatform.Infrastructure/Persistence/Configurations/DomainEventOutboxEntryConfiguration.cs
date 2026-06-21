using HealthPlatform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthPlatform.Infrastructure.Persistence.Configurations;

public sealed class DomainEventOutboxEntryConfiguration : IEntityTypeConfiguration<DomainEventOutboxEntry>
{
    public void Configure(EntityTypeBuilder<DomainEventOutboxEntry> builder)
    {
        builder.ToTable("domain_events");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).HasMaxLength(512).IsRequired();
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.OccurredAtUtc).IsRequired();
        builder.HasIndex(x => x.ProcessedAtUtc);
        builder.HasIndex(x => new { x.ProcessedAtUtc, x.OccurredAtUtc });
    }
}
