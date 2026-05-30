using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Contracts.Usher;
using UMS.Domain.Common;

namespace UMS.Application.Features.Ushers.Command.ApplyToSchedule
{

    public sealed record ApplyToScheduleCommand(
        Guid UsherId,
        string ExternalEventId,
        string ExternalScheduleId
    ) : IRequest<Result<ApplyToScheduleResponse>>;
    public sealed class ApplyToScheduleValidator
    : AbstractValidator<ApplyToScheduleCommand>
    {
        public ApplyToScheduleValidator()
        {
            RuleFor(x => x.UsherId).NotEmpty();
            RuleFor(x => x.ExternalEventId).NotEmpty();
            RuleFor(x => x.ExternalScheduleId).NotEmpty();
        }
    }
}
