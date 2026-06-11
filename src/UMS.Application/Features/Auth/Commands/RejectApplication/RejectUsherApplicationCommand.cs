using System;
using System.Collections.Generic;
using System.Text;

using FluentValidation;

using MediatR;

using UMS.Domain.Common;

namespace UMS.Application.Features.Auth.Commands.RejectApplication
{
    public sealed record RejectUsherApplicationCommand(
         Guid AdminId,
         Guid UsherId
     ) : IRequest<Result<Guid>>;
    public sealed class RejectUsherApplicationValidator
        : AbstractValidator<RejectUsherApplicationCommand>
    {
        public RejectUsherApplicationValidator()
        {
            RuleFor(x => x.UsherId).NotEmpty();
            RuleFor(x => x.AdminId).NotEmpty();
        }
    }
}