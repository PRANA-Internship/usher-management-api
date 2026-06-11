using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Coordinator.Usher
{
    public sealed record UsherScheduleItem(
      string ExternalScheduleId,
      string StartDate,
      string EndDate,
      bool IsOngoing,
      bool IsUpcoming,
      bool IsCompleted
  );

    public sealed record UsherEventItem(
        string ExternalEventId,
        string EventName,
        int SchedulesParticipated,
        IReadOnlyList<UsherScheduleItem> Schedules
    );

    public sealed record UsherEventHistoryResponse(
        Guid UsherId,
        string FullName,
        int TotalEvents,
        IReadOnlyList<UsherEventItem> Events
    );
}