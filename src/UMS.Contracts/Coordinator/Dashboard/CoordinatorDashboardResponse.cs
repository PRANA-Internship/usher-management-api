namespace UMS.Contracts.Coordinator.Dashboard
{
    public sealed record CoordinatorDashboardResponse(
        int TotalUshersConfirmed,
        int ActiveEvents
    );
}