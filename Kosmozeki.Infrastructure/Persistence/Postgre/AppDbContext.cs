using Kosmozeki.Domain.Notes;
using Kosmozeki.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}