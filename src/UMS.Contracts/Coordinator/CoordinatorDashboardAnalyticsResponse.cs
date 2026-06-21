using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Coordinator
{
    public sealed record CoordinatorDashboardAnalyticsResponse(
        int TotalUshersConfirmed,
        int ActiveEventsCount
    );
}
