using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using UMS.Application.Common.Interfaces;
using UMS.Application.Features.Coordinator.Queries.DashboardAnalytics;

namespace UMS.Infrastructure.Workers;

public class CoordinatorDashboardRefreshWorker : BackgroundService
{
    private readonly ILogger<CoordinatorDashboardRefreshWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    public CoordinatorDashboardRefreshWorker(ILogger<CoordinatorDashboardRefreshWorker> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CoordinatorDashboardRefreshWorker started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RefreshCacheAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing coordinator dashboard analytics cache.");
            }
            await Task.Delay(_interval, stoppingToken);
        }
        _logger.LogInformation("CoordinatorDashboardRefreshWorker stopping.");
    }

    private async Task RefreshCacheAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CoordinatorDashboardAnalyticsQueryHandler>();
        var query = new GetCoordinatorDashboardAnalyticsQuery();
        var result = await handler.Handle(query, ct);
        if (result.IsSuccess)
        {
            _logger.LogInformation("Coordinator dashboard analytics cache refreshed successfully.");
        }
        else
        {
            _logger.LogWarning("Failed to refresh coordinator dashboard analytics cache: {Error}", result.Error);
        }
    }
}