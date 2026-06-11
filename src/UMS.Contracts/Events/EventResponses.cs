using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Events
{
    public sealed record EventSummaryResponse(
    string EventId,
    string EventName,
    string? EventLogoLightUrl,
    string? EventLogoDarkUrl
);
    public sealed record ScheduleSummary(
    string EventScheduleId,
    string StartDate,
    string EndDate,
    string Venue,
    string City,
    string Country,
    bool IsOngoing,
    bool IsUpcoming,
    bool IsCompleted,
    // Assignment info — null if not yet assigned
    Guid? AssignedCoordinatorId,
    string? AssignedCoordinatorName,
    DateTimeOffset? AssignedAt
);

    public sealed record EventSchedulesResponse(
        string EventId,
        string EventName,
        IReadOnlyList<ScheduleSummary> Schedules
    );

}