using Kosmozeki.Domain.Shared;
using Kosmozeki.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kosmozeki.Infrastructure.DependencyInjection;

public static class InfrastructureDIExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventDispatcher, NoOpDomainEventDispatcher>();

        return services;
    }
}
