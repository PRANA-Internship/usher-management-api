using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using UMS.Application.Common.Interfaces;

namespace UMS.Infrastructure.Cache
{

    public sealed class RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger
    ) : ICacheService
    {
        private readonly IDatabase _db = redis.GetDatabase();

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
            where T : class
        {
            try
            {
                var value = await _db.StringGetAsync(key);
                if (value.IsNullOrEmpty) return null;

                return JsonSerializer.Deserialize<T>((string)value!, JsonOptions);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis GET failed for key {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(
            string key, T value, TimeSpan expiry, CancellationToken ct = default)
            where T : class
        {
            try
            {
                var json = JsonSerializer.Serialize(value, JsonOptions);
                await _db.StringSetAsync(key, json, expiry);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis SET failed for key {Key}", key);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken ct = default)
        {
            try
            {
                await _db.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis DELETE failed for key {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern, CancellationToken ct = default)
        {
            try
            {
                var server = redis.GetServer(redis.GetEndPoints().First());
                var keys = server.Keys(pattern: pattern).ToArray();
                if (keys.Length > 0)
                    await _db.KeyDeleteAsync(keys);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis DELETE by pattern failed for pattern {Pattern}", pattern);
            }
        }
    }

}