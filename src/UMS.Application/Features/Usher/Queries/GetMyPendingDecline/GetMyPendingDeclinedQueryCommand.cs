using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Contracts.Usher;
using UMS.Domain.Common;
using UMS.Domain.Enums;

namespace UMS.Application.Features.Ushers.Queries.GetMyPendingDecline
{
    public sealed record GetMyPendingDeclinedQuery(
    Guid UserId,
    MyScheduleStatusFilter? Filter,
    int Page,
    int Size
) : IRequest<Result<PagedScheduleResponse>>;

    public sealed class GetMyPendingDeclinedValidator
        : AbstractValidator<GetMyPendingDeclinedQuery>
    {
        public GetMyPendingDeclinedValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Filter).IsInEnum();
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.Size).InclusiveBetween(1, 50);
        }
    }


}
