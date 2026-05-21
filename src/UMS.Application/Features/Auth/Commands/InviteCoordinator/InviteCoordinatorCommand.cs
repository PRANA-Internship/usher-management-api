using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Common;

namespace UMS.Application.Features.Auth.Commands.InviteCoordinator
{
    public sealed record InviteCoordinatorCommand(
        Guid AdminId,
        string Email
        ) : IRequest<Result<bool>>;

    public sealed class InviteCoordinatorValidator
        : AbstractValidator<InviteCoordinatorCommand>
    {
        public InviteCoordinatorValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.AdminId).NotEmpty();
        }
    }
}
