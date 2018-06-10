using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Raidfelden.Discord.Bot.Utilities
{
    public class LazyConcurrentDictionary<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _concurrentDictionary;

        public LazyConcurrentDictionary()
        {
            _concurrentDictionary = new ConcurrentDictionary<TKey, Lazy<TValue>>();
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            var lazyResult = _concurrentDictionary.GetOrAdd(key, k => new Lazy<TValue>(() => valueFactory(k), LazyThreadSafetyMode.ExecutionAndPublication));
            return lazyResult.Value;
        }

	    public TValue AddOrUpdate(TKey key, TValue value, Func<TKey, TValue> valueFactory)
	    {
			var lazyResult = _concurrentDictionary.AddOrUpdate(key, new Lazy<TValue>(value), (key1, lazy) => new Lazy<TValue>(valueFactory(key)));
			return lazyResult.Value;
			
		}

	    public bool TryGetValue(TKey key, out TValue value)
	    {
		    value = default(TValue);
			var result = _concurrentDictionary.TryGetValue(key, out Lazy<TValue> lazyResult);
		    if (result)
		    {
			    value = lazyResult.Value;
		    }
		    return result;
	    }
    }
}
