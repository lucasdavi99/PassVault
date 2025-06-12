using Microsoft.Extensions.Caching.Memory;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;

namespace PassVault.Services
{
    public class CacheService : IDisposable
    {
        private readonly IMemoryCache _cache;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores;
        private readonly TimeSpan _defaultExpiry = TimeSpan.FromMinutes(10);

        // Cache keys constants
        private const string ACCOUNTS_KEY_PREFIX = "accounts_";
        private const string FOLDERS_KEY_PREFIX = "folders_";
        private const string SEARCH_KEY_PREFIX = "search_";

        public CacheService(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
            _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getItem, TimeSpan? expiry = null)
        {
            // Verificar se já existe no cache
            if (_cache.TryGetValue(key, out T cachedValue))
                return cachedValue;

            // Usar semáforo para evitar múltiplas execuções da mesma query
            var semaphore = _semaphores.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync();
            try
            {
                // Double-check após obter o lock
                if (_cache.TryGetValue(key, out cachedValue))
                    return cachedValue;

                // Executar a função e cachear o resultado
                var item = await getItem();
                var cacheExpiry = expiry ?? _defaultExpiry;

                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cacheExpiry,
                    SlidingExpiration = TimeSpan.FromMinutes(2),
                    Priority = CacheItemPriority.Normal,
                    Size = 1
                };

                _cache.Set(key, item, cacheEntryOptions);
                return item;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public T? Get<T>(string key)
        {
            return _cache.TryGetValue(key, out T value) ? value : default;
        }

        public void Set<T>(string key, T value, TimeSpan? expiry = null)
        {
            var cacheExpiry = expiry ?? _defaultExpiry;

            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheExpiry,
                SlidingExpiration = TimeSpan.FromMinutes(2),
                Priority = CacheItemPriority.Normal,
                Size = 1
            };

            _cache.Set(key, value, cacheEntryOptions);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);

            // Remover semáforo se existir
            if (_semaphores.TryRemove(key, out var semaphore))
            {
                semaphore.Dispose();
            }
        }

        public void RemoveByPattern(string pattern)
        {
            if (_cache is MemoryCache memoryCache)
            {
                var field = typeof(MemoryCache).GetField("_coherentState",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (field?.GetValue(memoryCache) is object coherentState)
                {
                    var entriesField = coherentState.GetType()
                        .GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (entriesField?.GetValue(coherentState) is IDictionary entries)
                    {
                        var keysToRemove = new List<string>();

                        foreach (DictionaryEntry entry in entries)
                        {
                            if (entry.Key.ToString()?.Contains(pattern) == true)
                            {
                                keysToRemove.Add(entry.Key.ToString()!);
                            }
                        }

                        foreach (var key in keysToRemove)
                        {
                            Remove(key);
                        }
                    }
                }
            }
        }

        // Métodos específicos para limpeza de cache por categoria
        public void ClearAccountsCache()
        {
            RemoveByPattern(ACCOUNTS_KEY_PREFIX);
        }

        public void ClearFoldersCache()
        {
            RemoveByPattern(FOLDERS_KEY_PREFIX);
        }

        public void ClearSearchCache()
        {
            RemoveByPattern(SEARCH_KEY_PREFIX);
        }

        public void ClearAll()
        {
            if (_cache is MemoryCache memoryCache)
            {
                // Método mais drástico - dispose e recriação seria necessária
                // Por ora, vamos limpar as categorias conhecidas
                ClearAccountsCache();
                ClearFoldersCache();
                ClearSearchCache();
            }
        }

        // Métodos helper para criar chaves de cache consistentes
        public static string CreateAccountsPageKey(int page) => $"{ACCOUNTS_KEY_PREFIX}page_{page}";
        public static string CreateFoldersPageKey(int page) => $"{FOLDERS_KEY_PREFIX}page_{page}";
        public static string CreateAccountsByFolderKey(int folderId, int page) => $"{ACCOUNTS_KEY_PREFIX}folder_{folderId}_page_{page}";
        public static string CreateSearchKey(string searchTerm, int page) => $"{SEARCH_KEY_PREFIX}{searchTerm.ToLower()}_page_{page}";

        // Estatísticas de cache (útil para debug)
        public CacheStatistics GetStatistics()
        {
            var stats = new CacheStatistics();

            if (_cache is MemoryCache memoryCache)
            {
                var field = typeof(MemoryCache).GetField("_coherentState",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (field?.GetValue(memoryCache) is object coherentState)
                {
                    var entriesField = coherentState.GetType()
                        .GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (entriesField?.GetValue(coherentState) is IDictionary entries)
                    {
                        stats.TotalEntries = entries.Count;

                        foreach (DictionaryEntry entry in entries)
                        {
                            var key = entry.Key.ToString() ?? "";

                            if (key.StartsWith(ACCOUNTS_KEY_PREFIX))
                                stats.AccountEntries++;
                            else if (key.StartsWith(FOLDERS_KEY_PREFIX))
                                stats.FolderEntries++;
                            else if (key.StartsWith(SEARCH_KEY_PREFIX))
                                stats.SearchEntries++;
                        }
                    }
                }
            }

            return stats;
        }

        // IDisposable implementation
        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                // Limpar todos os semáforos
                foreach (var semaphore in _semaphores.Values)
                {
                    semaphore.Dispose();
                }
                _semaphores.Clear();

                _disposed = true;
            }
        }
    }

    public class CacheStatistics
    {
        public int TotalEntries { get; set; }
        public int AccountEntries { get; set; }
        public int FolderEntries { get; set; }
        public int SearchEntries { get; set; }

        public override string ToString()
        {
            return $"Total: {TotalEntries}, Accounts: {AccountEntries}, Folders: {FolderEntries}, Search: {SearchEntries}";
        }
    }
}