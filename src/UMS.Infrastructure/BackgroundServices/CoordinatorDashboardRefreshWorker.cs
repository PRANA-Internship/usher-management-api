using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using UMS.Application.Common;
using UMS.Application.Common.Interfaces;
using UMS.Domain.Enums;
using UMS.Infrastructure.Cache;
namespace UMS.Infrastructure.BackgroundServices
{
    public sealed class CoordinatorDashboardRefreshWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<CoordinatorDashboardRefreshWorker> logger
    ) : BackgroundService
    {
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("CoordinatorDashboardRefreshWorker started.");

            await RefreshCacheAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_interval, stoppingToken);
                try
                {
                    await RefreshCacheAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error refreshing coordinator dashboard analytics cache.");
                }
            }
            logger.LogInformation("CoordinatorDashboardRefreshWorker stopping.");
        }

        private async Task RefreshCacheAsync(CancellationToken ct)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var repository = scope.ServiceProvider.GetRequiredService<ICoordinatorAnalyticsRepository>();
                var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
                var coordinatorIds = await userRepository.GetUserIdsByRoleAsync(UserRole.EVENT_COORDINATOR, ct);
                foreach (var coordinatorId in coordinatorIds)
                {
                    var data = await repository.GetCoordinatorDashboardAnalyticsAsync(coordinatorId, ct);
                    await cache.SetAsync(
                        CacheKeys.CoordinatorDashboardAnalytics(coordinatorId),
                        data,
                        CacheKeys.TTL.CoordinatorDashboardAnalytics,
                        ct);
                }

                logger.LogInformation("Coordinator dashboard analytics cache refreshed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to refresh coordinator dashboard analytics cache");
            }
        }
    }
}