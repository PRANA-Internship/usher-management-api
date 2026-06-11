using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Events
{
    public sealed record CoordinatorScheduleSummary(
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
      bool IsCompleted
  );

    public sealed record GetCoordinatorSchedulesResponse(
        IReadOnlyList<CoordinatorScheduleSummary> Schedules
    );

}