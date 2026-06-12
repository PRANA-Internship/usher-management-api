using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Domain.Common;
using UMS.Domain.Enums;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Auth.Commands.ResetPassword
{

    public sealed class ResetPasswordCommandHandler(
        IEmailVerificationTokenRepository tokenRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<ResetPasswordCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(
            ResetPasswordCommand command,
            CancellationToken cancellationToken)
        {
            var token = await tokenRepository.GetByTokenAsync(command.Token, cancellationToken);

            if (token is null || token.TokenType != TokenType.PasswordReset)
                return AuthErrors.InvalidResetToken;

            if (!token.IsValid)
                return AuthErrors.InvalidResetToken;

            if (token.IsUsed)
                return AuthErrors.TokenAlreadyUsed;

            var user = await userRepository.GetByIdAsync(token.UserId, cancellationToken);
            if (user is null)
                return AuthErrors.UserNotFound;

            var passwordHash = passwordHasher.Hash(command.Password);

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                user.SetPassword(passwordHash);
                user.RevokeRefreshToken();
                token.MarkAsUsed();

                await userRepository.UpdateAsync(user, cancellationToken);
                await tokenRepository.UpdateAsync(token, cancellationToken);
            }, cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}