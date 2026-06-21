using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Contracts.Usher;
using UMS.Domain.Common;

public sealed record GetMyAnalyticsQuery(Guid UserId)
    : IRequest<Result<UsherAnalyticsSummary>>;

public sealed class GetMyAnalyticsQueryHandler(
    IUsherRepository usherRepository,
    IUsherAnalyticsRepository analyticsRepository,
    ICacheService cache
) : IRequestHandler<GetMyAnalyticsQuery, Result<UsherAnalyticsSummary>>
{
    private static string CacheKey(Guid usherId) => $"usher:analytics:{usherId}";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    public async Task<Result<UsherAnalyticsSummary>> Handle(
        GetMyAnalyticsQuery query, CancellationToken cancellationToken)
    {
        var usher = await usherRepository.GetByUserIdAsync(
            query.UserId, cancellationToken);

        if (usher is null)
            return Result<UsherAnalyticsSummary>.Failure(
                new Error("USHER_004", "Usher not found."));

        var cacheKey = CacheKey(usher.Id);

        var cached = await cache.GetAsync<UsherAnalyticsSummary>(
            cacheKey, cancellationToken);

        if (cached is not null)
            return Result<UsherAnalyticsSummary>.Success(cached);

        var data = await analyticsRepository
            .GetUsherAnalyticsAsync(usher.Id, cancellationToken);

        await cache.SetAsync(cacheKey, data, CacheDuration, cancellationToken);

        return Result<UsherAnalyticsSummary>.Success(data);
    }
}