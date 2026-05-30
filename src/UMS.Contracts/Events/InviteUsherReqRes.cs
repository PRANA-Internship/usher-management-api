using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Events
{
    public sealed record InviteUsherRequest(Guid UsherId, string eventId, string scheduleId);
    public sealed record InviteUsherResponse(
    Guid InvitationId,
    Guid UsherId,
    string UsherFullName,
    string UsherEmail,
    string Status,
    DateTimeOffset InvitedAt
   );
}
