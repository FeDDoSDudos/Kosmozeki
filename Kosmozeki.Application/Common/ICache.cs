using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Application.Common;

public interface ICache
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetKeysByPrefixAsync(string prefix, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetByPrefixAsync<T>(string prefix, CancellationToken ct = default) where T : class;
}
