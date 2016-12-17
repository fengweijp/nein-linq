using System;
using System.Collections.Generic;
using System.Threading;

namespace NeinLinq
{
    class ObjectCache<TKey, TValue> : IDisposable
    {
        readonly Dictionary<TKey, TValue> cache = new Dictionary<TKey, TValue>();

        readonly ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (valueFactory == null)
                throw new ArgumentNullException(nameof(valueFactory));

            cacheLock.EnterUpgradeableReadLock();

            try
            {
                var value = default(TValue);
                if (cache.TryGetValue(key, out value))
                    return value;

                cacheLock.EnterWriteLock();

                try
                {
                    value = valueFactory(key);
                    cache.Add(key, value);

                    return value;
                }
                finally
                {
                    cacheLock.ExitWriteLock();
                }
            }
            finally
            {
                cacheLock.ExitUpgradeableReadLock();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                cacheLock.Dispose();
            }
        }
    }
}
