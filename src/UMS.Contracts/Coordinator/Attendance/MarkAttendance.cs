using System;
using System.Collections.Generic;
using System.Text;

using UMS.Domain.Enums;

namespace UMS.Contracts.Coordinator.Attendance
{

    public sealed record MarkAttendanceRequest(
        Guid UsherId,
        string eventId,
        string scheduleId,
        int Status,
        DayStatus DayStatus

    );
    public sealed record MarkAttendanceResponse(
    Guid AttendanceId,
    Guid UsherId,
    string FullName,
    DateOnly AttendanceDate,
    string DayStatus,
    string Status,
    int Score,
    bool IsMarked,
    DateTimeOffset MarkedAt
);


}