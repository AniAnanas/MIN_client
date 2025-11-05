using Client.Services.Interfaces;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;

namespace Client.Managers
{
    public class CacheManager
    {
        private readonly ConcurrentDictionary<string, object> _cache = new();
        private readonly string _cacheDirectory;
        private readonly ILoggingService _logger;

        public CacheManager(ILoggingService logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Cache");

            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
            }
        }
        
        public T? Get<T>(string key)
        {
            try
            {
                if (_cache.TryGetValue(key, out var value))
                {
                    if (value is T typedValue)
                    {
                        _logger.Info($"Cache hit for key: {key}");
                        return typedValue;
                    }
                }

                // Try to load from disk
                var filePath = GetCacheFilePath(key);
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var cachedValue = JsonSerializer.Deserialize<T>(json);
                    if (cachedValue != null)
                    {
                        _cache[key] = cachedValue;
                        _logger.Info($"Loaded from disk cache: {key}");
                        return cachedValue;
                    }
                }

                _logger.Info($"Cache miss for key: {key}");
                return default;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting cache item {key}: {ex.Message}");
                return default;
            }
        }

        public void Set<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                _cache[key] = value;

                // Save to disk
                var json = JsonSerializer.Serialize(value);
                var filePath = GetCacheFilePath(key);
                File.WriteAllText(filePath, json);

                _logger.Info($"Cached item: {key}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error setting cache item {key}: {ex.Message}");
            }
        }

        public void Remove(string key)
        {
            try
            {
                _cache.TryRemove(key, out _);

                var filePath = GetCacheFilePath(key);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                _logger.Info($"Removed cache item: {key}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error removing cache item {key}: {ex.Message}");
            }
        }

        public void Clear()
        {
            try
            {
                _cache.Clear();

                if (Directory.Exists(_cacheDirectory))
                {
                    Directory.Delete(_cacheDirectory, true);
                    Directory.CreateDirectory(_cacheDirectory);
                }

                _logger.Info("Cache cleared");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error clearing cache: {ex.Message}");
            }
        }

        private string GetCacheFilePath(string key)
        {
            var fileName = $"{key.GetHashCode()}.json";
            return Path.Combine(_cacheDirectory, fileName);
        }
    }
}
