using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Contracts.Admin.Dashboard;
using UMS.Domain.Common;
using UMS.Application.Common;

namespace UMS.Application.Features.Admin.Queries.Dashboard
{

    public sealed record GetAdminDashboardQuery : IRequest<Result<AdminDashboardResponse>>;


    public sealed class GetAdminDashboardQueryHandler(
        IUsherRepository usherRepository,
        IEventsApiClient eventsApiClient,
        IUsherPerformanceReviewRepository reviewRepository,
        ICacheService cache
    ) : IRequestHandler<GetAdminDashboardQuery, Result<AdminDashboardResponse>>
    {

        public async Task<Result<AdminDashboardResponse>> Handle(
            GetAdminDashboardQuery query,
            CancellationToken cancellationToken)
        {

            var cached = await cache.GetAsync<AdminDashboardResponse>(
                CacheKeys.AdminDashboard, cancellationToken);

            if (cached is not null)
                return Result<AdminDashboardResponse>.Success(cached);

            var totalApproved = await usherRepository
                .CountApprovedAsync(cancellationToken);

            var totalPending = await usherRepository
                .CountPendingAsync(cancellationToken);

            var averageRating = await reviewRepository
                .GetAverageRatingAsync(cancellationToken);

            int activeEventsCount;
            try
            {
                var events = await eventsApiClient.GetEventsAsync(cancellationToken);

                activeEventsCount = events.Count;
            }
            catch
            {
                activeEventsCount = 0;
            }

            var response = new AdminDashboardResponse(
                TotalApprovedUshers: totalApproved,
                TotalPendingUshers: totalPending,
                ActiveEventsCount: activeEventsCount,
                AveragePerformanceRating: Math.Round(averageRating, 2)
            );

            await cache.SetAsync(
                CacheKeys.AdminDashboard, response, CacheKeys.TTL.AdminDashboardDuration, cancellationToken);

            return Result<AdminDashboardResponse>.Success(response);
        }
    }




















}