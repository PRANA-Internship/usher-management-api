using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;

using UMS.Application.Common;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Coordinator;
using UMS.Domain.Common;
using UMS.Domain.Enums;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Coordinator.Queries.GetCoordinatorDashboardAnalytics
{
    public sealed record GetCoordinatorDashboardAnalyticsQuery(Guid UserId)
        : IRequest<Result<CoordinatorDashboardAnalyticsResponse>>;

    public sealed class GetCoordinatorDashboardAnalyticsQueryHandler(
        IUserRepository userRepository,
        ICoordinatorAnalyticsRepository analyticsRepository,
        ICacheService cache
    ) : IRequestHandler<GetCoordinatorDashboardAnalyticsQuery, Result<CoordinatorDashboardAnalyticsResponse>>
    {
        public async Task<Result<CoordinatorDashboardAnalyticsResponse>> Handle(
            GetCoordinatorDashboardAnalyticsQuery query,
            CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(
                query.UserId,
                cancellationToken);

            if (user is null || user.Role != UserRole.EVENT_COORDINATOR)
                return UserErrors.NotFound;

            var cacheKey = CacheKeys.CoordinatorAnalytics(user.Id);

            var cached = await cache.GetAsync<CoordinatorDashboardAnalyticsResponse>(
                cacheKey, cancellationToken);

            if (cached is not null)
                return Result<CoordinatorDashboardAnalyticsResponse>.Success(cached);

            var data = await analyticsRepository
                .GetCoordinatorAnalyticsAsync(user.Id, cancellationToken);

            await cache.SetAsync(cacheKey, data, CacheKeys.TTL.CoordinatorAnalytics, cancellationToken);

            return Result<CoordinatorDashboardAnalyticsResponse>.Success(data);
        }
    }
}