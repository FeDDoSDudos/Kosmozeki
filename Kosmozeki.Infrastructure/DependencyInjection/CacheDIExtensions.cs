using Kosmozeki.Application.Common;
using Kosmozeki.Infrastructure.Cache;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Infrastructure.DependencyInjection;

public static class CacheDIExtensions
{
    public static IServiceCollection AddCache(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICache, MemoryCacheService>();

        return services;
    }
}
