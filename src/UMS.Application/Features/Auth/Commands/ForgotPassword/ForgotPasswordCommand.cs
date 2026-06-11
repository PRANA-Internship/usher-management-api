using System;
using System.Collections.Generic;
using System.Text;

using FluentValidation;

using MediatR;

using UMS.Domain.Common;

namespace UMS.Application.Features.Auth.Commands.ForgotPassword
{

    public sealed record ForgotPasswordCommand(string Email) : IRequest<Result<bool>>;

    public sealed class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }

}