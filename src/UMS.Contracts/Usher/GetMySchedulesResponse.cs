using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Usher
{
    public sealed record ScheduleItem(
    string ExternalScheduleId,
    string ExternalEventId,
    string EventName,
    string StartDate,
    string EndDate,
    string Venue,
    string City,
    string Country,
    bool IsOngoing,
    bool IsUpcoming,
    bool IsCompleted,
    string Type,
    string Status,
    Guid? InvitationId
);

    public sealed record PagedScheduleResponse(
        IReadOnlyList<ScheduleItem> Items,
        int TotalCount,
        int Page,
        int Size,
        int TotalPages
    );

    public sealed record ConfirmedScheduleItem(
        string ExternalScheduleId,
        string ExternalEventId,
        string EventName,
        string StartDate,
        string EndDate,
        string Venue,
        string City,
        string Country,
        bool IsOngoing,
        bool IsUpcoming,
        bool IsCompleted,
        string Type,
        DateTimeOffset ConfirmedAt,
        Guid? InvitationId
    );
    public sealed record PagedConfirmedScheduleResponse(
        IReadOnlyList<ConfirmedScheduleItem> Items,
        int TotalCount,
        int Page,
        int Size,
        int TotalPages
    );
}
