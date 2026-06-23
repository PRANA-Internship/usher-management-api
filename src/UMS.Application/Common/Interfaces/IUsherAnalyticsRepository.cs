using System;
using System.Collections.Generic;
using System.Text;

using UMS.Domain.Entities;
using UMS.Domain.Enums;
public interface IUsherAnalyticsRepository
{
    Task<UsherAnalyticsSummary> GetUsherAnalyticsAsync(
        Guid usherId, CancellationToken ct = default);

    Task<int> CountCompletedEventsAsync(
        Guid usherId, CancellationToken ct = default);
}