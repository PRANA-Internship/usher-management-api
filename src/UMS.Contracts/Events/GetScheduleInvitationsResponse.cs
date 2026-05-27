using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Events
{

    public sealed record UsherInvitationSummary(
        Guid InvitationId,
        Guid UsherId,
        string UsherFullName,
        string UsherEmail,
        string UsherPhone,
        string Status,
        DateTimeOffset InvitedAt,
        DateTimeOffset? RespondedAt
    );

    public sealed record GetScheduleInvitationsResponse(
       string ExternalScheduleId,
       string ExternalEventId,
       IReadOnlyList<UsherInvitationSummary> Items,
       int TotalCount,
       int Page,
       int Size,
       int TotalPages,
       int TotalAccepted,
       int TotalDeclined,
       int TotalPending,
       string? AppliedStatus
    );
}
