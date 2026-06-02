using Kosmozeki.Application.Common;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace Kosmozeki.Infrastructure.Cache;

public sealed class MemoryCacheService : ICache
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, byte> _keys = new();


    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };

        options.RegisterPostEvictionCallback((k, _, _, _) =>
        {
            if (k is string sk)
                _keys.TryRemove(sk, out _);
        });

        _cache.Set(key, value, options);
        _keys[key] = 0;

        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var keysToRemove = _keys.Keys
            .Where(k => k.StartsWith(prefix, StringComparison.Ordinal))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _cache.Remove(key);
        _keys.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> GetKeysByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var keys = _keys.Keys
            .Where(k => k.StartsWith(prefix, StringComparison.Ordinal))
            .OrderBy(k => k)
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(keys);
    }

    public Task<IReadOnlyList<T>> GetByPrefixAsync<T>(string prefix, CancellationToken ct = default) where T : class
    {
        var values = _keys.Keys
            .Where(k => k.StartsWith(prefix, StringComparison.Ordinal))
            .Select(k =>
            {
                _cache.TryGetValue(k, out T? value);
                return value;
            })
            .Where(v => v is not null)
            .Cast<T>()
            .ToList();

        return Task.FromResult<IReadOnlyList<T>>(values);
    }
}