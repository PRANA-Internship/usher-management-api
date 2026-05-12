using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Contracts.Auth;
using UMS.Domain.Common;

namespace UMS.Application.Features.Auth.Commands.Login
{
    public sealed record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponse>>;
}
