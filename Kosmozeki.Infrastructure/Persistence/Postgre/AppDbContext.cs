using Kosmozeki.Domain.Notes;
using Kosmozeki.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Kosmozeki.Infrastructure.Persistence.Postgre;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<SharedNote> Notes => Set<SharedNote>();
    public DbSet<OutboxEntry> OutboxEntries => Set<OutboxEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.Entity<SharedNote>()
            .Ignore(x => x.DomainEvents);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}