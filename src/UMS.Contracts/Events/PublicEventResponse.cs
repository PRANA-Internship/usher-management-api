using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Events
{
    public sealed record PublicScheduleResponse(
    string EventScheduleId,
    string StartDate,
    string EndDate,
    string Venue,
    string City,
    string Country,
    bool IsOngoing,
    bool IsUpcoming,
    bool IsCompleted
);

    public sealed record PublicEventResponse(
        string EventId,
        string EventName,
        string? EventLogoLightUrl,
        string? EventLogoDarkUrl,
        IReadOnlyList<PublicScheduleResponse> Schedules
    );
}