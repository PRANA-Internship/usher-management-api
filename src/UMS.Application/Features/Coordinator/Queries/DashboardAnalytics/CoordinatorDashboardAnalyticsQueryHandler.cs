using MediatR;

using UMS.Application.Common;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Coordinator.Dashboard;
using UMS.Domain.Common;
using UMS.Infrastructure.Cache;
namespace UMS.Application.Features.Coordinator.Queries.DashboardAnalytics;

public sealed class CoordinatorDashboardAnalyticsQueryHandler(
    ICoordinatorAnalyticsRepository analyticsRepository,
    ICacheService cacheService
) : IRequestHandler<GetCoordinatorDashboardAnalyticsQuery, Result<CoordinatorDashboardResponse>>
{
    public async Task<Result<CoordinatorDashboardResponse>> Handle(GetCoordinatorDashboardAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var cached = await cacheService.GetAsync<CoordinatorDashboardResponse>(CacheKeys.CoordinatorDashboardAnalytics(request.CoordinatorId), cancellationToken);
        if (cached is not null)
            return Result<CoordinatorDashboardResponse>.Success(cached);

        var response = await analyticsRepository.GetCoordinatorDashboardAnalyticsAsync(request.CoordinatorId, cancellationToken);
        await cacheService.SetAsync(CacheKeys.CoordinatorDashboardAnalytics(request.CoordinatorId), response, CacheKeys.TTL.CoordinatorDashboardAnalytics, cancellationToken);
        return Result<CoordinatorDashboardResponse>.Success(response);
    }
}