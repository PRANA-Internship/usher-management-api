using System;
using System.Collections.Generic;
using System.Text;

using FluentValidation;

using MediatR;

using UMS.Contracts.Coordinator.Usher;
using UMS.Domain.Common;

namespace UMS.Application.Features.Coordinator.Queries.UsherEventHistory
{
    public sealed record UsherEventHistoryQuery(
      Guid UsherId
  ) : IRequest<Result<UsherEventHistoryResponse>>;

    public sealed class UsherEventHistoryValidator
        : AbstractValidator<UsherEventHistoryQuery>
    {
        public UsherEventHistoryValidator()
        {
            RuleFor(x => x.UsherId).NotEmpty();
        }
    }
}