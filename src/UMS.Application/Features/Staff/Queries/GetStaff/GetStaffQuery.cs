using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Contracts.Staff;
using UMS.Domain.Common;
using UMS.Domain.Enums;

namespace UMS.Application.Features.Staff.Queries.GetStaff
{
    public sealed record GetStaffQuery(
        int Page,
        int Size,
        UserRole? Role,
        UserStatus? Status,
        string? SearchName
    ) : IRequest<Result<GetStaffResponse>>;

    public sealed class GetStaffValidator : AbstractValidator<GetStaffQuery>
    {
        public GetStaffValidator()
        {
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.Size).InclusiveBetween(1, 50);

            When(x => x.Role is not null, () =>
                RuleFor(x => x.Role)
                    .Must(r => r == UserRole.ADMIN || r == UserRole.EVENT_COORDINATOR)
                    .WithMessage("Role filter must be ADMIN or EVENT_COORDINATOR."));

            When(x => x.SearchName is not null, () =>
                RuleFor(x => x.SearchName)
                    .MinimumLength(2)
                    .MaximumLength(100));
        }
    }
}
