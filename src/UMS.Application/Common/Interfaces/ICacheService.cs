using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Application.Common.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken ct = default) where T : class;
        Task RemoveAsync(string key, CancellationToken ct = default);
        Task RemoveByPatternAsync(string pattern, CancellationToken ct = default);
    }
}