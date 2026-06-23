using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Usher
{
    public sealed record ApplyToScheduleRequest(
       Guid UsherId,
       string ExternalEventId,
       string ExternalScheduleId
   );
    public sealed record ApplyToScheduleResponse(
    Guid ApplicationId,
    string ExternalScheduleId,
    string ExternalEventId,
    string Status,
    DateTimeOffset AppliedAt
);
}