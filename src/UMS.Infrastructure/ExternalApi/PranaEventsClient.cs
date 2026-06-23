using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using UMS.Application.Common.Interfaces;
using UMS.Application.Common.Models;
using UMS.Infrastructure.Cache;

namespace UMS.Infrastructure.ExternalApi
{
    public sealed class PranaEventsClient(
        HttpClient httpClient,
        ICacheService cache,
        ILogger<PranaEventsClient> logger,
        IHttpContextAccessor httpContextAccessor
    ) : IEventsApiClient
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<IReadOnlyList<ExternalEventDto>> GetEventsAsync(
            CancellationToken ct = default)
        {
            var cached = await cache.GetAsync<List<ExternalEventDto>>(
                CacheKeys.AllEvents, ct);

            if (cached is not null)
                return cached;

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "events/public");
                var authHeader = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authHeader.Substring("Bearer ".Length));
                }
                var response = await httpClient.SendAsync(request, ct);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Failed to fetch events: {StatusCode}", response.StatusCode);
                    return new List<ExternalEventDto>();
                }

                var json = await response.Content.ReadAsStringAsync(ct);
                var result = JsonSerializer.Deserialize<List<ExternalEventDto>>(
                    json, JsonOptions) ?? [];

                await cache.SetAsync(CacheKeys.AllEvents, result, CacheKeys.TTL.Events, ct);

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to fetch events from Prana Events API.");
                throw;
            }
        }

        public async Task<ExternalPaginatedEventsDto> GetPaginatedEventsAsync(
            int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var cacheKey = CacheKeys.PaginatedEvents(pageNumber, pageSize);
            var cached = await cache.GetAsync<ExternalPaginatedEventsDto>(cacheKey, ct);
            if (cached is not null)
                return cached;

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"events/public?pageNumber={pageNumber}&pageSize={pageSize}");
                var authHeader = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authHeader.Substring("Bearer ".Length));
                }
                var response = await httpClient.SendAsync(request, ct);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Failed to fetch paginated events: {StatusCode}", response.StatusCode);
                    return new ExternalPaginatedEventsDto(new List<ExternalEventDto>(), pageNumber, pageSize, false, false);
                }

                var json = await response.Content.ReadAsStringAsync(ct);
                var events = JsonSerializer.Deserialize<List<ExternalEventDto>>(json, JsonOptions) ?? [];

                var result = new ExternalPaginatedEventsDto(events, pageNumber, pageSize, events.Count == pageSize, pageNumber > 1);

                await cache.SetAsync(cacheKey, result, CacheKeys.TTL.Events, ct);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to fetch paginated events from Prana Events API.");
                throw;
            }
        }

        public async Task<ExternalEventDetailDto?> GetEventByIdAsync(
            string eventId, CancellationToken ct = default)
        {
            var cacheKey = CacheKeys.EventById(eventId);

            var cached = await cache.GetAsync<ExternalEventDetailDto>(cacheKey, ct);
            if (cached is not null)
                return cached;

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"events/public/{eventId}");
                var authHeader = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authHeader.Substring("Bearer ".Length));
                }
                var response = await httpClient.SendAsync(request, ct);
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync(ct);
                var result = JsonSerializer.Deserialize<ExternalEventDetailDto>(
                    json, JsonOptions);

                if (result is not null)
                    await cache.SetAsync(cacheKey, result, CacheKeys.TTL.Events, ct);

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to fetch event {EventId} from Prana API.", eventId);
                throw;
            }
        }

        public async Task<ScheduleDto?> GetScheduleByIdAsync(
            string eventId, string scheduleId, CancellationToken ct = default)
        {
            var detail = await GetEventByIdAsync(eventId, ct);
            return detail?.Schedules.FirstOrDefault(s => s.EventScheduleId == scheduleId);
        }
    }

}