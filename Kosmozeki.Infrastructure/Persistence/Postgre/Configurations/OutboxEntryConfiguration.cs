using Kosmozeki.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kosmozeki.Infrastructure.Persistence.Postgre.Configurations;

public sealed class OutboxEntryConfiguration : IEntityTypeConfiguration<OutboxEntry>
{
    public void Configure(EntityTypeBuilder<OutboxEntry> builder)
    {
        builder.ToTable("outbox_entries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.EntityType)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.EntityId)
            .IsRequired();

        builder.Property(x => x.Operation)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Payload)
            .IsRequired();

        builder.Property(x => x.RetryCount)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ProcessedAt);

        builder.HasIndex(x => x.ProcessedAt);
        builder.HasIndex(x => x.CreatedAt);
    }
}