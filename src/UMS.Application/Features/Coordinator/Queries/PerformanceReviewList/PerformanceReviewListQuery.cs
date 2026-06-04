using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Common;
using UMS.Contracts.Coordinator.Performance;

namespace UMS.Application.Features.Coordinator.Queries.PerformanceReviewList
{
    public sealed record PerformanceReviewListQuery(
      Guid CoordinatorId,
      string ExternalEventId,
      string ExternalScheduleId
  ) : IRequest<Result<PerformanceReviewListResponse>>;

    public sealed class GetPerformanceReviewListValidator
        : AbstractValidator<PerformanceReviewListQuery>
    {
        public GetPerformanceReviewListValidator()
        {
            RuleFor(x => x.CoordinatorId).NotEmpty();
            RuleFor(x => x.ExternalEventId).NotEmpty();
            RuleFor(x => x.ExternalScheduleId).NotEmpty();
        }
    }

}
