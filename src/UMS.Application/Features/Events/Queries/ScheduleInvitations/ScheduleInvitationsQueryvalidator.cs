using System;
using System.Collections.Generic;
using System.Text;

using FluentValidation;

using MediatR;

using UMS.Contracts.Events;
using UMS.Domain.Common;
using UMS.Domain.Enums;

namespace UMS.Application.Features.Events.Queries.GetScheduleInvitations
{
    public sealed record GetScheduleInvitationsQuery(
           string ExternalScheduleId,
           string ExternalEventId,
           Guid CoordinatorId,
           int Page,
           int Size,
           InvitationStatus? Status
) : IRequest<Result<GetScheduleInvitationsResponse>>;
    public sealed class GetScheduleInvitationsValidator
    : AbstractValidator<GetScheduleInvitationsQuery>
    {
        public GetScheduleInvitationsValidator()
        {
            RuleFor(x => x.ExternalScheduleId).NotEmpty();
            RuleFor(x => x.ExternalEventId).NotEmpty();
            RuleFor(x => x.CoordinatorId).NotEmpty();

            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page must be at least 1.");

            RuleFor(x => x.Size)
                .InclusiveBetween(1, 50)
                .WithMessage("Size must be between 1 and 50.");

            When(x => x.Status is not null, () =>
                RuleFor(x => x.Status)
                    .IsInEnum()
                    .WithMessage("Invalid status value."));
        }
    }

}