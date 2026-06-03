using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Contracts.Coordinator;
using UMS.Domain.Common;

namespace UMS.Application.Features.Coordinator.Queries.GetConfirmed
{
    public sealed record ConfirmedUshersRosterQuery(
     string ExternalScheduleId,
     Guid CoordinatorId,
     int Page,
     int Size
 ) : IRequest<Result<PagedConfirmedRosterResponse>>;

    public sealed class GetConfirmedRosterValidator
        : AbstractValidator<ConfirmedUshersRosterQuery>
    {
        public GetConfirmedRosterValidator()
        {
            RuleFor(x => x.ExternalScheduleId).NotEmpty();
            RuleFor(x => x.CoordinatorId).NotEmpty();
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.Size).InclusiveBetween(1, 50);
        }
    }
}
