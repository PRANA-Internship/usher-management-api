using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Coordinator
{
    public sealed record AvailableUsherItem(
     Guid UsherId,
     string FullName,
     string Email,
     string Phone,
     string City,
     IReadOnlyList<string> Languages,
     IReadOnlyList<string> Sectors
 );

    public sealed record PagedAvailableUshersResponse(
        IReadOnlyList<AvailableUsherItem> Items,
        int TotalCount,
        int Page,
        int Size,
        int TotalPages
    );
}
