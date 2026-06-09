using System;

namespace UMS.Contracts.Admin
{
    public sealed record AdminDashboardAnalyticsResponse(
        int TotalApprovedUshers,
        int TotalPendingUshers,
        int ActiveEventsCount,
        double AveragePerformanceRating
    );
}
