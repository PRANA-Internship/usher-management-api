using System;
using System.Collections.Generic;
using System.Text;

using FluentValidation;

using MediatR;

using UMS.Contracts.Staff;
using UMS.Domain.Common;
using UMS.Domain.Enums;

namespace UMS.Application.Features.Staff.Commands.CreateStaff
{

    public sealed record CreateStaffCommand(
        Guid AdminId,
        string FullName,
        string Email,
        string Phone,
        UserRole Role
    ) : IRequest<Result<CreateStaffResponse>>;
    public sealed class CreateStaffValidator : AbstractValidator<CreateStaffCommand>
    {
        public CreateStaffValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
            RuleFor(x => x.AdminId).NotEmpty();
            RuleFor(x => x.Role)
                .Must(r => r == UserRole.ADMIN || r == UserRole.EVENT_COORDINATOR)
                .WithMessage("Role must be ADMIN or EVENT_COORDINATOR.");
        }
    }
}