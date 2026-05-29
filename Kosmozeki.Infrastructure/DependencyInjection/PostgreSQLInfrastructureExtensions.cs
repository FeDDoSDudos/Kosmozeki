using Kosmozeki.Application.Common;
using Kosmozeki.Domain.Notes;
using Kosmozeki.Domain.Shared;
using Kosmozeki.Domain.Sync;
using Kosmozeki.Infrastructure.Persistence.Postgre;
using Kosmozeki.Infrastructure.Persistence.Postgre.Repositories;
using Kosmozeki.Infrastructure.ReadDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kosmozeki.Infrastructure.DependencyInjection;

public static class PostgreSQLInfrastructureExtensions
{
    public static IServiceCollection AddPostgreSQL(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            object value = options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<INoteRepository, EfNoteRepository>();
        services.AddScoped<IOutboxRepository, EfOutboxRepository>();
        services.AddScoped<IReadDb, PostgresReadDb>();

        return services;
    }
}