using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

using UMS.Application.Common.Interfaces;
using UMS.Contracts.Coordinator;
using UMS.Domain.Common;
using UMS.Infrastructure.Cache;

namespace UMS.Application.Features.Coordinator.Queries.DashboardAnalytics
{
    public sealed record DashboardAnalyticsQuery(Guid CoordinatorId, bool ForceRefresh = false)
        : IRequest<Result<CoordinatorDashboardAnalyticsResponse>>;

    public sealed class DashboardAnalyticsQueryHandler(
        IScheduleAssignmentRepository assignmentRepository,
        IEventsApiClient eventsApiClient,
        ICacheService cache,
        IServiceProvider serviceProvider)
        : IRequestHandler<DashboardAnalyticsQuery, Result<CoordinatorDashboardAnalyticsResponse>>
    {
        public async Task<Result<CoordinatorDashboardAnalyticsResponse>> Handle(
            DashboardAnalyticsQuery query,
            CancellationToken cancellationToken)
        {
            var cacheKey = CacheKeys.CoordinatorDashboard(query.CoordinatorId);

            if (!query.ForceRefresh)
            {
                var cachedResponse = await cache.GetAsync<CoordinatorDashboardAnalyticsResponse>(cacheKey, cancellationToken);
                if (cachedResponse is not null)
                {
                    return Result<CoordinatorDashboardAnalyticsResponse>.Success(cachedResponse);
                }
            }

            var assignments = await assignmentRepository.GetByCoordinatorIdAsync(
                query.CoordinatorId, cancellationToken);

            if (assignments.Count == 0)
            {
                var emptyResponse = new CoordinatorDashboardAnalyticsResponse(0, 0);
                await cache.SetAsync(cacheKey, emptyResponse, CacheKeys.TTL.CoordinatorDashboardDuration, cancellationToken);
                return Result<CoordinatorDashboardAnalyticsResponse>.Success(emptyResponse);
            }

            var scheduleIds = assignments
                .Select(a => a.ExternalScheduleId)
                .Distinct()
                .ToList();

            // Parallelize per‑schedule count queries while avoiding shared DbContext concurrency.
            var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            var countTasks = scheduleIds.Select(async scheduleId =>
            {
                using var scope = scopeFactory.CreateScope();
                var appRepo = scope.ServiceProvider.GetRequiredService<IUsherScheduleApplicationRepository>();
                var invRepo = scope.ServiceProvider.GetRequiredService<IUsherInvitationRepository>();
                var approved = await appRepo.CountApprovedByScheduleAsync(scheduleId, cancellationToken);
                var accepted = await invRepo.CountAcceptedByScheduleAsync(scheduleId, cancellationToken);
                return approved + accepted;
            });
            var confirmedCounts = await Task.WhenAll(countTasks);
            var totalUshersConfirmed = confirmedCounts.Sum();

            var uniqueEventIds = assignments
                .Select(a => a.ExternalEventId)
                .ToHashSet();

            int activeEventsCount;
            try
            {
                var publicEvents = await eventsApiClient.GetEventsAsync(cancellationToken);
                activeEventsCount = publicEvents.Count(e => uniqueEventIds.Contains(e.EventId));
            }
            catch
            {
                activeEventsCount = 0;
            }

            var response = new CoordinatorDashboardAnalyticsResponse(totalUshersConfirmed, activeEventsCount);

            await cache.SetAsync(cacheKey, response, CacheKeys.TTL.CoordinatorDashboardDuration, cancellationToken);

            return Result<CoordinatorDashboardAnalyticsResponse>.Success(response);
        }
    }
}