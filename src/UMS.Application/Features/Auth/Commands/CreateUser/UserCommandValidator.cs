using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Contracts.Auth;
using UMS.Domain.Common;
using UMS.Domain.Enums;

namespace UMS.Application.Features.Auth.Commands.CreateUser
{
    //again for creating users just for devlopment
    public sealed record CreateUserCommand(
     string FullName,
     string Email,
     string Phone,
     string Password,
     UserRole Role
 ) : IRequest<Result<AuthResponse>>;



    public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Phone).NotEmpty();
            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.");
            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("Invalid role.")
                .WithMessage("Cannot seed a GUEST user.");
        }
    }

}
