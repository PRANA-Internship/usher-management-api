using System;

namespace UMS.Contracts.Events;

public sealed record RemoveCoordinatorRequest(string eventId, string scheduleId);

public sealed record RemoveCoordinatorResponse(
    Guid AssignmentId,
    string ExternalScheduleId,
    string ExternalEventId
);