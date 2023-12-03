
namespace TinyURL
{
    public interface ICache<TKey, TValue>
    {
        TValue Get(TKey key);
        Task<TValue> GetOrCreateAsync(TKey key, Func<Task<TValue>> valueFactory);
        void Set(TKey key, TValue value);
    }
}
