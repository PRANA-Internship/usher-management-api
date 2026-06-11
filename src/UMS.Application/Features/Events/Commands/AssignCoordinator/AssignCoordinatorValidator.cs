using System;
using System.Collections.Generic;
using System.Text;

using FluentValidation;

using MediatR;

using UMS.Contracts.Events;
using UMS.Domain.Common;

namespace UMS.Application.Features.Events.Commands.AssignCoordinator
{

    public sealed record AssignCoordinatorCommand(
        string ExternalEventId,
        string ExternalScheduleId,
        Guid CoordinatorId,
        Guid AdminId
    ) : IRequest<Result<AssignCoordinatorResponse>>;
    public sealed class AssignCoordinatorValidator
    : AbstractValidator<AssignCoordinatorCommand>
    {
        public AssignCoordinatorValidator()
        {
            RuleFor(x => x.ExternalEventId).NotEmpty();
            RuleFor(x => x.ExternalScheduleId).NotEmpty();
            RuleFor(x => x.CoordinatorId).NotEmpty();
            RuleFor(x => x.AdminId).NotEmpty();
        }
    }
}