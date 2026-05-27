using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Contracts.Events;
using UMS.Domain.Common;

namespace UMS.Application.Features.Events.Commands.InviteUsher
{
    public sealed record InviteUsherCommand(
     string ExternalScheduleId,
     string ExternalEventId,
     Guid UsherId,
     Guid CoordinatorId
 ) : IRequest<Result<InviteUsherResponse>>;

    public sealed class InviteUsherValidator : AbstractValidator<InviteUsherCommand>
    {
        public InviteUsherValidator()
        {
            RuleFor(x => x.ExternalScheduleId).NotEmpty();
            RuleFor(x => x.ExternalEventId).NotEmpty();
            RuleFor(x => x.UsherId).NotEmpty();
            RuleFor(x => x.CoordinatorId).NotEmpty();
        }
    }

}
