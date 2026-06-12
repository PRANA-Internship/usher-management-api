using System;
using System.Collections.Generic;
using System.Text;

using FluentValidation;

using MediatR;

using UMS.Domain.Common;
using UMS.Domain.Enums;

namespace UMS.Application.Features.Coordinator.Queries.GetScheduleRoster
{
    public sealed record GetScheduleRosterQuery(
       Guid CoordinatorId,
       string ExternalEventId,
       string ExternalScheduleId,
       CoordinatorScheduleFilter Filter,
       int Page,
       int Size
   ) : IRequest<Result<object>>;

    public sealed class GetScheduleRosterValidator
        : AbstractValidator<GetScheduleRosterQuery>
    {
        public GetScheduleRosterValidator()
        {
            RuleFor(x => x.CoordinatorId).NotEmpty();
            RuleFor(x => x.ExternalEventId).NotEmpty();
            RuleFor(x => x.ExternalScheduleId).NotEmpty();
            RuleFor(x => x.Filter).IsInEnum();
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.Size).InclusiveBetween(1, 50);
        }
    }
}