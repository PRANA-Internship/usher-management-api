using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Contracts.Auth;
using UMS.Domain.Common;

namespace UMS.Application.Features.Auth.Commands.RegisterCoordinator
{
    public sealed record RegisterCoordinatorCommand(
       string Token,
       string FullName,
       string Phone,
       string Password,
       string ConfirmPassword
   ) : IRequest<Result<RegisterCoordinatorResponse>>;

    public sealed class RegisterCoordinatorValidator
        : AbstractValidator<RegisterCoordinatorCommand>
    {
        public RegisterCoordinatorValidator()
        {
            RuleFor(x => x.Token).NotEmpty();

            RuleFor(x => x.FullName)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(100);

            RuleFor(x => x.Phone)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(30);

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
