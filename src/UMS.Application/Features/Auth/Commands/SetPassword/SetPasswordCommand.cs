using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Common;
namespace UMS.Application.Features.Auth.Commands.SetPassword
{
    public sealed record SetPasswordCommand(
        string Token,
        string Password,
        string ConfirmPassword
    ) : IRequest<Result<bool>>;
    public sealed class SetPasswordValidator : AbstractValidator<SetPasswordCommand>
    {
        public SetPasswordValidator()
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
