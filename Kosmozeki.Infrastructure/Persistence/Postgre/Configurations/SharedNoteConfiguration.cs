using Kosmozeki.Domain.Notes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kosmozeki.Infrastructure.Persistence.Postgre.Configurations;

public sealed class SharedNoteConfiguration : IEntityTypeConfiguration<SharedNote>
{
    public void Configure(EntityTypeBuilder<SharedNote> builder)
    {
        builder.ToTable("notes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.RoomId)
            .IsRequired();

        builder.Property(x => x.AuthorPlayerId)
            .IsRequired();

        builder.Property(x => x.Content)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(x => x.Visibility)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Version)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.Property(x => x.IsDirty)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(256);

        builder.HasIndex(x => x.RoomId);
        builder.HasIndex(x => new { x.RoomId, x.UpdatedAt });
    }
}