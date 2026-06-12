using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Admin.Dashboard
{
    public sealed record MonthlyAttendanceData(
       int Year,
       int Month,
       int TotalSessions,
       int TotalMarked,
       int TotalScore,
       int MaxPossibleScore
   );
    public sealed class MonthlyAttendanceRaw
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalSessions { get; set; }
        public int TotalMarked { get; set; }
        public int TotalScore { get; set; }
        public int MaxPossibleScore { get; set; }
    }
}