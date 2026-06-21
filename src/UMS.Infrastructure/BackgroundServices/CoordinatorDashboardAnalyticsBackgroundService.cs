using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using UMS.Application.Common.Interfaces;
using UMS.Application.Features.Coordinator.Queries.DashboardAnalytics;
using UMS.Domain.Enums;

namespace UMS.Infrastructure.BackgroundServices
{
    public class CoordinatorDashboardAnalyticsBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CoordinatorDashboardAnalyticsBackgroundService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

        public CoordinatorDashboardAnalyticsBackgroundService(IServiceProvider serviceProvider, ILogger<CoordinatorDashboardAnalyticsBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CoordinatorDashboardAnalyticsBackgroundService started.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                    var coordinatorIds = await userRepository.GetUserIdsByRoleAsync(UserRole.EVENT_COORDINATOR, stoppingToken);
                    foreach (var coordinatorId in coordinatorIds)
                    {
                        await sender.Send(new DashboardAnalyticsQuery(coordinatorId, ForceRefresh: true), stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while refreshing coordinator dashboard analytics cache.");
                }

                await Task.Delay(Interval, stoppingToken);
            }

            _logger.LogInformation("CoordinatorDashboardAnalyticsBackgroundService stopping.");
        }
    }
}