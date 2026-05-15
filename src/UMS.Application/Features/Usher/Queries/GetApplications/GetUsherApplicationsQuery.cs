using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Contracts.Usher;
using UMS.Domain.Common;
using UMS.Domain.Enums;

namespace UMS.Application.Features.Ushers.Queries.GetApplications
{

    public sealed record GetUsherApplicationsQuery(
        int Page,
        int Size,
        ApprovalStatus? Status
    ) : IRequest<Result<GetUsherApplicationsResponse>>;
}
