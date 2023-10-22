using StoreApp.Models;

namespace StoreApp.Utilities;

public interface ICacheProvider
{
    Task<T> GetCache<T>(string key);
    Task<T> SetCache<T>(string key, T value);
    Task RemoveCache(string key);
}
