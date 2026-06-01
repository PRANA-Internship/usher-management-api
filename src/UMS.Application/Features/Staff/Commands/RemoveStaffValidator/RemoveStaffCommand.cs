using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Common;

namespace UMS.Application.Features.Staff.Commands.RemoveStaffValidator
{
    public sealed record RemoveStaffCommand(
     Guid AdminId,
     Guid StaffUserId
 ) : IRequest<Result<bool>>;

    public sealed class RemoveStaffValidator : AbstractValidator<RemoveStaffCommand>
    {
        public RemoveStaffValidator()
        {
            RuleFor(x => x.AdminId).NotEmpty();
            RuleFor(x => x.StaffUserId).NotEmpty();
        }
    }
}
