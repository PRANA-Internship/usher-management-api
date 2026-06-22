using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using UMS.Application.Common;
using UMS.Application.Common.Interfaces;

namespace UMS.Infrastructure.BackgroundServices
{
    public sealed class ReportCacheRefreshWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<ReportCacheRefreshWorker> logger
    ) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            await RefreshAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

                await RefreshAsync(stoppingToken);
            }
        }

        private async Task RefreshAsync(CancellationToken ct)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();

                var usherRepository = scope.ServiceProvider
                    .GetRequiredService<IUsherRepository>();
                var analyticsRepository = scope.ServiceProvider
                    .GetRequiredService<IUsherAnalyticsRepository>();
                var cache = scope.ServiceProvider
                    .GetRequiredService<ICacheService>();


                var usherIds = await usherRepository.GetAllApprovedIdsAsync(ct);

                foreach (var usherId in usherIds)
                {
                    var data = await analyticsRepository
                        .GetUsherAnalyticsAsync(usherId, ct);

                    await cache.SetAsync(
                        CacheKeys.UsherAnalytics(usherId),
                        data,
                        CacheKeys.TTL.UsherAnalytics,
                        ct);

                    logger.LogInformation(
                        "Refreshed analytics cache for usher {UsherId}", usherId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Report cache refresh failed");
            }
        }
    }
}