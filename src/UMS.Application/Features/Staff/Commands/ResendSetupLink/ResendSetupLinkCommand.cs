using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Contracts.Staff;
using UMS.Domain.Common;

namespace UMS.Application.Features.Staff.Commands.ResendSetupLink
{
    public sealed record ResendSetupLinkCommand(
          Guid AdminId,
          Guid StaffUserId
      ) : IRequest<Result<ResendSetupLinkResponse>>;
    public sealed class ResendSetupLinkValidator
    : AbstractValidator<ResendSetupLinkCommand>
    {
        public ResendSetupLinkValidator()
        {
            RuleFor(x => x.AdminId).NotEmpty();
            RuleFor(x => x.StaffUserId).NotEmpty();
        }
    }
}
