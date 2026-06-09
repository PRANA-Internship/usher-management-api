using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Admin;
using UMS.Domain.Common;
using UMS.Infrastructure.Cache;
using Microsoft.Extensions.DependencyInjection;

namespace UMS.Application.Features.Admin.Queries
{
    public sealed class GetAdminDashboardAnalyticsQueryHandler(
        IUsherRepository usherRepository,
        IUsherPerformanceReviewRepository reviewRepository,
        IEventsApiClient eventsApiClient,
        ICacheService cache,
        IServiceScopeFactory scopeFactory
    ) : IRequestHandler<GetAdminDashboardAnalyticsQuery, Result<AdminDashboardAnalyticsResponse>>
    {
        private const string CacheKey = "admin:dashboard:analytics";

        public async Task<Result<AdminDashboardAnalyticsResponse>> Handle(
            GetAdminDashboardAnalyticsQuery query,
            CancellationToken cancellationToken)
        {
            try
            {
                AdminDashboardAnalyticsResponse? cached = null;
                try
                {
                    cached = await cache.GetAsync<AdminDashboardAnalyticsResponse>(CacheKey, cancellationToken);
                }
                catch { /* Log cache failure but proceed to DB */ }

                if (cached is not null)
                    return Result<AdminDashboardAnalyticsResponse>.Success(cached);

                var approvedTask = usherRepository.CountApprovedAsync(cancellationToken);
                var pendingTask = usherRepository.CountPendingAsync(cancellationToken);
                var ratingTask = reviewRepository.GetAverageRatingAsync(cancellationToken);

                await Task.WhenAll(approvedTask, pendingTask, ratingTask);

                var totalApproved = await approvedTask;
                var totalPending = await pendingTask;
                var averageRating = await ratingTask;

                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = scopeFactory.CreateScope();
                        var scopedEventsApi = scope.ServiceProvider.GetRequiredService<IEventsApiClient>();
                        var scopedCache = scope.ServiceProvider.GetRequiredService<ICacheService>();

                        var allEvts = await scopedEventsApi.GetEventsAsync(CancellationToken.None);
                        if (allEvts == null || !allEvts.Any()) return;

                        int activeCount = 0;
                        await Parallel.ForEachAsync(allEvts, new ParallelOptions { MaxDegreeOfParallelism = 5 }, async (evt, ct) =>
                        {
                            try
                            {
                                var detail = await scopedEventsApi.GetEventByIdAsync(evt.EventId, ct);
                                if (detail?.Schedules.Any(s => s.IsOngoing || s.IsUpcoming) == true)
                                {
                                    Interlocked.Increment(ref activeCount);
                                }
                            }
                            catch
                            {
                            }
                        });

                        var newResponse = new AdminDashboardAnalyticsResponse(
                            TotalApprovedUshers: totalApproved,
                            TotalPendingUshers: totalPending,
                            ActiveEventsCount: activeCount,
                            AveragePerformanceRating: Math.Round(averageRating, 2)
                        );

                        await scopedCache.SetAsync(CacheKey, newResponse, CacheKeys.TTL.AdminDashboardDuration, CancellationToken.None);
                    }
                    catch
                    {
                    }
                });

                var response = new AdminDashboardAnalyticsResponse(
                    TotalApprovedUshers: totalApproved,
                    TotalPendingUshers: totalPending,
                    ActiveEventsCount: 0,
                    AveragePerformanceRating: Math.Round(averageRating, 2)
                );

                return Result<AdminDashboardAnalyticsResponse>.Success(response);
            }
            catch (Exception)
            {
                return Result<AdminDashboardAnalyticsResponse>.Failure(
                    new Error("DASHBOARD_001", "Failed to retrieve dashboard analytics."));
            }
        }
    }
}
