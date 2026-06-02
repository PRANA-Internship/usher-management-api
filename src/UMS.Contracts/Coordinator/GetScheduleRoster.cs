using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Coordinator
{
    public sealed record ScheduleApplicationItem(
      Guid ApplicationId,
      Guid UsherId,
      string FullName,
      string Email,
      string Phone,
      string City,
      DateTimeOffset AppliedAt,
      string Status
  );
    public sealed record ScheduleInvitationItem(
      Guid InvitationId,
      Guid UsherId,
      string FullName,
      string Email,
      string Phone,
      string City,
      DateTimeOffset SentAt,
      string Status
  );
    public sealed record ConfirmedRosterItem(
    Guid UsherId,
    string FullName,
    string Email,
    string Phone,
    string City,
    string Type,
    DateTimeOffset ConfirmedAt
);

    public sealed record PagedApplicationResponse(
    IReadOnlyList<ScheduleApplicationItem> Items,
       int TotalCount,
       int Page,
       int Size,
       int TotalPages
);

    public sealed record PagedInvitationResponse(
        IReadOnlyList<ScheduleInvitationItem> Items,
            int TotalCount,
            int Page,
            int Size,
            int TotalPages
    );
    public sealed record PagedConfirmedRosterResponse(
    IReadOnlyList<ConfirmedRosterItem> Items,
    int TotalCount,
    int Page,
    int Size,
    int TotalPages
);

}
