# TinyURL

The implemented caching solution for optimizing bursts of redirections in your URL shortening service uses a combination of a `ConcurrentDictionary` for thread-safe storage and an LRU (Least Recently Used) eviction policy to limit the cache size. Here's a brief overview of this approach:

### Size-Limitation Approach: LRU Eviction Policy

- **Implementation:** The cache size is limited by the `_maxSize` parameter. When new items are added and the cache exceeds this size, the least recently accessed items are removed to make room for new ones.
- **Method:** This is achieved by tracking the last accessed time of each item in the cache. When the cache needs to be trimmed, items with the oldest `LastAccessed` timestamps are evicted first.

### Advantages

1. **Concurrency and Multi-threading Safety:** `ConcurrentDictionary` is inherently thread-safe for add, update, and remove operations, making it suitable for a high-concurrency environment.
2. **Efficiency for High-Access Items:** The LRU strategy keeps frequently or recently accessed items in the cache, which is beneficial for scenarios where certain URLs are accessed more often than others.
3. **Simplicity and Predictability:** The LRU policy is straightforward to implement and understand. It provides predictable behavior in terms of which items will be evicted.

### Disadvantages

1. **Potential Overhead in Eviction Logic:** The LRU approach requires sorting items based on the `LastAccessed` timestamp during eviction, which could introduce overhead, especially when the cache size is large.
2. **Not Ideal for Uniform Access Patterns:** If access patterns are uniform across all URLs, the LRU policy might not provide significant benefits over other eviction strategies.
3. **Manual Eviction Handling:** The cache eviction logic needs to be invoked manually, typically during the addition of new items, which adds complexity to the cache management.

### Why This Approach?

This approach was chosen for its balance between efficiency, ease of implementation, and suitability for the expected access pattern in a URL shortening service. In such a service, it's common for some URLs to be accessed more frequently than others, making LRU an effective strategy for keeping the most relevant items in cache. Additionally, the use of `ConcurrentDictionary` aligns well with the need for thread-safe operations in a web service environment where multiple threads may access the cache concurrently.

### Conclusion

The LRU-based cache implemented using `ConcurrentDictionary` and manual eviction logic offers a practical balance for a URL shortening service, optimizing cache hit rates for frequently accessed URLs while ensuring thread safety and managing memory usage effectively. The choice of LRU is driven by the nature of web traffic, which often exhibits a pattern where some URLs are far more popular than others.
