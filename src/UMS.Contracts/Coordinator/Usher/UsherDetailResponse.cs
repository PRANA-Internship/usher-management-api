using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Coordinator.Usher
{
    public sealed record UsherDetailResponse(
    Guid UsherId,
    string FullName,
    string Email,
    string Phone,


    int TotalConfirmedEvents,
    double OverallAttendanceRate,
    double AveragePerformanceRating
);
}