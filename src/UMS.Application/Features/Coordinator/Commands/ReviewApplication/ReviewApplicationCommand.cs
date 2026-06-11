using System;
using System.Collections.Generic;
using System.Text;

using FluentValidation;

using MediatR;

using UMS.Domain.Common;

namespace UMS.Application.Features.Coordinator.Commands.ReviewApplication
{

    public sealed record ReviewApplicationCommand(
        Guid CoordinatorId,
        Guid ApplicationId,
        bool Approve
    ) : IRequest<Result<bool>>;

    public sealed class ReviewApplicationValidator
        : AbstractValidator<ReviewApplicationCommand>
    {
        public ReviewApplicationValidator()
        {
            RuleFor(x => x.CoordinatorId).NotEmpty();
            RuleFor(x => x.ApplicationId).NotEmpty();
        }
    }
}