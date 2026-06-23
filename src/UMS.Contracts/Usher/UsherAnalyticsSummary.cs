using System;
using System.Collections.Generic;
using System.Text;

public sealed record UsherAnalyticsSummary(
    double AverageScore,
    double AttendanceRate,
    int EventsDone
);