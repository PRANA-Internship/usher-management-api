using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Admin.Dashboard
{
    public sealed record AdminDashboardResponse(
     int TotalApprovedUshers,
     int TotalPendingUshers,
     int ActiveEventsCount,
     double AveragePerformanceRating
 );
    public sealed record AttendanceTrendPoint(
    int Year,
    int Month,
    string MonthLabel,
    double AveragePercentage,
    int TotalSessions,
    int TotalMarked
);

    public sealed record AttendanceTrendResponse(
        IReadOnlyList<AttendanceTrendPoint> Trend,
        double OverallAverage
    );

}