using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Admin
{
    public sealed record CoordinatorSummary(
    Guid UserId,
    string FullName,
    string Email,
    string Phone,
    DateTimeOffset CreatedAt
);
    public sealed record GetCoordinatorsResponse(
    IReadOnlyList<CoordinatorSummary> Items,
    int TotalCount,
    int Page,
    int Size,
    int TotalPages,
    string? AppliedSearch
);

}