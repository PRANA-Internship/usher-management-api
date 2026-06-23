using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Contracts.Coordinator.Dashboard;
using UMS.Domain.Common;
using UMS.Infrastructure.Cache;

namespace UMS.Application.Features.Coordinator.Queries.DashboardAnalytics;

public sealed class CoordinatorDashboardAnalyticsQueryHandler : IRequestHandler<GetCoordinatorDashboardAnalyticsQuery, Result<CoordinatorDashboardResponse>>
{
    private readonly IUsherRepository _usherRepository;
    private readonly IEventsApiClient _eventsApiClient;
    private readonly ICacheService _cacheService;

    public CoordinatorDashboardAnalyticsQueryHandler(IUsherRepository usherRepository, IEventsApiClient eventsApiClient, ICacheService cacheService)
    {
        _usherRepository = usherRepository;
        _eventsApiClient = eventsApiClient;
        _cacheService = cacheService;
    }

    public async Task<Result<CoordinatorDashboardResponse>> Handle(GetCoordinatorDashboardAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var cached = await _cacheService.GetAsync<CoordinatorDashboardResponse>(CacheKeys.CoordinatorDashboardAnalytics, cancellationToken);
        if (cached is not null)
            return Result<CoordinatorDashboardResponse>.Success(cached);

        var totalUshersConfirmed = await _usherRepository.CountApprovedAsync(cancellationToken);
        var events = await _eventsApiClient.GetEventsAsync(cancellationToken);
        var activeEvents = events?.Count ?? 0;

        var response = new CoordinatorDashboardResponse(totalUshersConfirmed, activeEvents);
        await _cacheService.SetAsync(CacheKeys.CoordinatorDashboardAnalytics, response, CacheKeys.TTL.CoordinatorDashboardAnalytics, cancellationToken);
        return Result<CoordinatorDashboardResponse>.Success(response);
    }
}