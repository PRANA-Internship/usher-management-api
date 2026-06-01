using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Staff
{
    public sealed record StaffSummary(
        Guid UserId,
        string FullName,
        string Email,
        string Phone,
        string Role,
        string Status,
        DateTimeOffset CreatedAt
    );

    public sealed record GetStaffResponse(
        IReadOnlyList<StaffSummary> Items,
        int TotalCount,
        int Page,
        int Size,
        int TotalPages,
        string? AppliedRole,
        string? AppliedStatus,
        string? AppliedSearch
    );
}
