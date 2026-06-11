using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Domain.Common;
using UMS.Domain.Enums;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Staff.Commands.SetUpPassword
{
    public sealed class SetupPasswordCommandHandler(
    IEmailVerificationTokenRepository tokenRepository,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork
) : IRequestHandler<SetupPasswordCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(
            SetupPasswordCommand command,
            CancellationToken cancellationToken)
        {
            var token = await tokenRepository.GetByTokenAsync(command.Token, cancellationToken);

            if (token is null || token.TokenType != TokenType.StaffSetup)
                return StaffErrors.InvalidSetupToken;

            if (!token.IsValid)
                return StaffErrors.InvalidSetupToken;

            if (token.IsUsed)
                return StaffErrors.TokenAlreadyUsed;

            var user = await userRepository.GetByIdAsync(token.UserId, cancellationToken);
            if (user is null)
                return StaffErrors.InvalidSetupToken;

            var passwordHash = passwordHasher.Hash(command.Password);

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                user.ActivateWithPassword(passwordHash);
                token.MarkAsUsed();

                await userRepository.UpdateAsync(user, cancellationToken);
                await tokenRepository.UpdateAsync(token, cancellationToken);
            }, cancellationToken);

            return Result<bool>.Success(true);
        }
    }

}