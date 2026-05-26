using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Events
{
    public sealed record AssignCoordinatorRequest(string eventId, string scheduleId, Guid CoordinatorId);

    public sealed record AssignCoordinatorResponse(
        Guid AssignmentId,
        string ExternalScheduleId,
        string ExternalEventId,
        Guid CoordinatorId,
        string CoordinatorName,
        string CoordinatorEmail,
        DateTimeOffset AssignedAt
    );
}
