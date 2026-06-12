using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Application.Common.Models
{
    public sealed record ExternalEventDto(
    string EventId,
    string EventName,
    string? EventLogoLightUrl,
    string? EventLogoDarkUrl
);
    public sealed record ScheduleDto(
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
    public sealed record ExternalEventDetailDto(
      ExternalEventDto Event,
      IReadOnlyList<ScheduleDto> Schedules
  );

    public sealed record ExternalPaginatedEventsDto(
        IReadOnlyList<ExternalEventDto> Events,
        int PageNumber,
        int PageSize,
        bool HasNextPage,
        bool HasPreviousPage
    );
}