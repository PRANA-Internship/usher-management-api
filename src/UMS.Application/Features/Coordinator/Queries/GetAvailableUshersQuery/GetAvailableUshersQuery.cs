using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Contracts.Coordinator;
using UMS.Domain.Common;

namespace UMS.Application.Features.Coordinator.Queries.GetAvailableUshersQuery
{

    public sealed record GetAvailableUshersQuery(
        string ExternalScheduleId,
        string ExternalEventId,
        Guid CoordinatorId,
        int Page,
        int Size
    ) : IRequest<Result<PagedAvailableUshersResponse>>;

    public sealed class GetAvailableUshersValidator
        : AbstractValidator<GetAvailableUshersQuery>
    {
        public GetAvailableUshersValidator()
        {
            RuleFor(x => x.ExternalScheduleId).NotEmpty();
            RuleFor(x => x.ExternalEventId).NotEmpty();
            RuleFor(x => x.CoordinatorId).NotEmpty();
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.Size).InclusiveBetween(1, 50);
        }
    }
}
