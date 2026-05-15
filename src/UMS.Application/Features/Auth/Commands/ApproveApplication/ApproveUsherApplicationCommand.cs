using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using UMS.Domain.Common;
using MediatR;


namespace UMS.Application.Features.Auth.Commands.ApproveApplication
{
    public sealed record ApproveUsherApplicationCommand(
          Guid AdminId,
          Guid UsherId
      ) : IRequest<Result<Guid>>;
    public sealed class ApproveUsherApplicationValidator
        : AbstractValidator<ApproveUsherApplicationCommand>
    {
        public ApproveUsherApplicationValidator()
        {
            RuleFor(x => x.UsherId).NotEmpty();
            RuleFor(x => x.AdminId).NotEmpty();
        }
    }
}
