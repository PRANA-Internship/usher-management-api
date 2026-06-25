namespace UMS.Contracts.Coordinator;

public sealed record CoordinatorDashboardAnalyticsResponse(
    int TotalUshersConfirmed,
    int ActiveEvents
);