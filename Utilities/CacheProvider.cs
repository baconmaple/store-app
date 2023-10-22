using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using StackExchange.Redis;
using StoreApp.Models;
using System;

namespace StoreApp.Utilities;

public class CacheProvider : ICacheProvider
{
    private readonly IDistributedCache _cache;

    public CacheProvider(IDistributedCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<T> GetCache<T>(string key)
    {
        var value = await _cache.GetStringAsync(key);

        if (string.IsNullOrEmpty(value))
            return default;

        return JsonConvert.DeserializeObject<T>(value);
    }

    public async Task<T> SetCache<T>(string key, T value)
    {
        await _cache.SetStringAsync(key, JsonConvert.SerializeObject(value));

        return await GetCache<T>(key);
    }

    public async Task RemoveCache(string key)
    {
        await _cache.RemoveAsync(key);
    }
}
