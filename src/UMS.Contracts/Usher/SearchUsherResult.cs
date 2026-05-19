using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Enums;

namespace UMS.Contracts.Usher
{
   

    public sealed record SearchUsherResponse(
        IReadOnlyList<UsherApplicationSummary> Items,
        int TotalCount
    );
}
