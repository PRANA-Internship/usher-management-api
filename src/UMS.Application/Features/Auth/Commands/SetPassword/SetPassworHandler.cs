using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Auth;
using UMS.Domain.Common;
using UMS.Domain.Entities;
using UMS.Domain.Enums;
using static UMS.Domain.Common.Error;
namespace UMS.Application.Features.Auth.Commands.SetPassword
{
    public sealed class SetPasswordCommandHandler(
        IEmailVerificationTokenRepository tokenRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<SetPasswordCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(
            SetPasswordCommand command,
            CancellationToken cancellationToken)
        {
            var verificationToken = await tokenRepository.GetByTokenAsync(
                command.Token, cancellationToken);

            if (verificationToken is null || !verificationToken.IsValid)
                return UsherErrors.InvalidToken;

            if (verificationToken.IsUsed)
                return UsherErrors.TokenAlreadyUsed;

            var user = await userRepository.GetByIdAsync(
                verificationToken.UserId, cancellationToken);

            if (user is null)
                return UsherErrors.NotFound;

            var passwordHash = passwordHasher.Hash(command.Password);

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                user.VerifyEmailAndSetPassword(passwordHash);
                verificationToken.MarkAsUsed();

                await userRepository.UpdateAsync(user, cancellationToken);
                await tokenRepository.UpdateAsync(verificationToken, cancellationToken);
            }, cancellationToken);

            return Result<bool>.Success(true);
        }
    }

}
