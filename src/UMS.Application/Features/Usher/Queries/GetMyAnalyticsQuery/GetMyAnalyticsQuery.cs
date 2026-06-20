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
    IUsherAnalyticsRepository analyticsRepository
) : IRequestHandler<GetMyAnalyticsQuery, Result<UsherAnalyticsSummary>>
{
    public async Task<Result<UsherAnalyticsSummary>> Handle(
        GetMyAnalyticsQuery query, CancellationToken cancellationToken)
    {

        var usher = await usherRepository.GetByUserIdAsync(
            query.UserId, cancellationToken);

        if (usher is null)
            return Result<UsherAnalyticsSummary>.Failure(
                new Error("USHER_004", "Usher not found."));


        var data = await analyticsRepository
            .GetUsherAnalyticsAsync(usher.Id, cancellationToken);

        return Result<UsherAnalyticsSummary>.Success(data);
    }
}