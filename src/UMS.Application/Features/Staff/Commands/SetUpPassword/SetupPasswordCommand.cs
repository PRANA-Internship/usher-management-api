using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Common;

namespace UMS.Application.Features.Staff.Commands.SetUpPassword
{

    public sealed record SetupPasswordCommand(
        string Token,
        string Password,
        string ConfirmPassword
    ) : IRequest<Result<bool>>;
    public sealed class SetupPasswordValidator : AbstractValidator<SetupPasswordCommand>
    {
        public SetupPasswordValidator()
        {
            RuleFor(x => x.Token).NotEmpty();
            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.");
            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("Passwords do not match.");
        }
    }
}
