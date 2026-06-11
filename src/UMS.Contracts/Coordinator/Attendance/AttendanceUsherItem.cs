using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Coordinator.Attendance
{
    public sealed record AttendanceUsherItem(
     Guid UsherId,
     Guid AttendanceId,
     string FullName,
     string Phone,
     string City,
     bool IsMarked,
     string Status,
     int Score,
     DateTimeOffset? MarkedAt
 );
    public sealed record AttendanceSheetResponse(
    string ExternalScheduleId,
    string ExternalEventId,
    DateOnly AttendanceDate,
    string DayStatus,
    int TotalConfirmed,
    int TotalMarked,
    int TotalNotMarked,
    IReadOnlyList<AttendanceUsherItem> Ushers
);

}