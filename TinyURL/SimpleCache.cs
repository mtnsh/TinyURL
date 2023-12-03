namespace TinyURL;

using System;
using System.Collections.Concurrent;
using System.Linq;

public class SimpleCache<TKey, TValue> : ICache<TKey, TValue>
{
    private readonly ConcurrentDictionary<TKey, CacheItem<TValue>> _cache;
    private readonly int _maxSize;
    private readonly object _lock = new();
    private readonly ConcurrentDictionary<TKey, Task<CacheItem<TValue>>> _pendingRequests;
    private readonly TimeSpan _negativeCachingDuration;

    public SimpleCache(int maxSize = 100) // Default max size
    {
        _maxSize = maxSize;
        _cache = new ConcurrentDictionary<TKey, CacheItem<TValue>>();
        _pendingRequests = new ConcurrentDictionary<TKey, Task<CacheItem<TValue>>>();
        _negativeCachingDuration = TimeSpan.FromMinutes(5); // Default to 5 minutes
    }
    public async Task<TValue> GetOrCreateAsync(TKey key, Func<Task<TValue>> valueFactory)
    {
        if (_cache.TryGetValue(key, out CacheItem<TValue> cachedItem))
        {
            // Check for a negative cache hit (null value in cache)
            if (cachedItem.Value == null && cachedItem.LastAccessed.Add(_negativeCachingDuration) > DateTime.UtcNow)
            {
                // Negative cache hit and it's still valid
                return default(TValue);
            }
            else
            {
                // Negative cache hit has expired, remove it
                _cache.TryRemove(key, out _);
            }

            // Positive cache hit
            return cachedItem.Value;
        }

        // Handle pending requests
        var pendingRequest = _pendingRequests.GetOrAdd(key, _ => FetchAndCacheValueAsync(key, valueFactory));
        var completedItem = await pendingRequest;

        if (completedItem == null)
        {
            // The request resulted in a null value - store this as a negative cache entry
            SetNegativeCacheEntry(key);
        }
        if (completedItem != null) return completedItem.Value;
        return default(TValue);
    }

    private void SetNegativeCacheEntry(TKey key)
    {
        var negativeCacheItem = new CacheItem<TValue> { Value = default(TValue)};
        negativeCacheItem.UpdateLastAccessed();
        _cache[key] = negativeCacheItem;
    }

    private async Task<CacheItem<TValue>> FetchAndCacheValueAsync(TKey key, Func<Task<TValue>> valueFactory)
    {
        try
        {
            var value = await valueFactory();

            // Check if the fetched value is null or default (indicating not found)
            if (EqualityComparer<TValue>.Default.Equals(value, default(TValue)))
            {
                _pendingRequests.TryRemove(key, out _);
                return null; // represent a 'not found' scenario
            }

            var newItem = new CacheItem<TValue> { Value = value };
            newItem.UpdateLastAccessed();
            _cache[key] = newItem;
            return newItem;
        }
        catch (Exception ex)
        {
            // Log and handle the exception
            _pendingRequests.TryRemove(key, out _);
            throw;
        }
    }

    public TValue Get(TKey key)
    {
        if (_cache.TryGetValue(key, out CacheItem<TValue> item))
        {
            // Update the item's last accessed time
            item.UpdateLastAccessed();
            return item.Value;
        }

        return default(TValue);
    }

    public void Set(TKey key, TValue value)
    {
        var newItem = new CacheItem<TValue> { Value = value };

        _cache.AddOrUpdate(key, newItem, (_, existingVal) =>
        {
            existingVal.Value = value;
            existingVal.UpdateLastAccessed();
            return existingVal;
        });

        EnsureSizeLimit();
    }

    private void EnsureSizeLimit()
    {
        lock (_lock)
        {
            if (_cache.Count <= _maxSize) return;

            var oldestItems = _cache.OrderBy(pair => pair.Value.LastAccessed)
                                    .Take(_cache.Count - _maxSize)
                                    .Select(pair => pair.Key)
                                    .ToList();

            foreach (TKey? key in oldestItems)
            {
                _cache.TryRemove(key, out _);
            }
        }
    }

    private class CacheItem<T>
    {
        public T Value { get; set; }
        public DateTime LastAccessed { get; private set; }

        public CacheItem()
        {
            LastAccessed = DateTime.UtcNow;
        }

        public void UpdateLastAccessed()
        {
            LastAccessed = DateTime.UtcNow;
        }
    }
}