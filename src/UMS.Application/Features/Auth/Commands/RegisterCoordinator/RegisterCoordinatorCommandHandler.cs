using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Auth;
using UMS.Domain.Common;
using UMS.Domain.Enums;
using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Auth.Commands.RegisterCoordinator
{

    public sealed class RegisterCoordinatorCommandHandler(
        IEmailVerificationTokenRepository tokenRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
         ITokenService tokenService,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<RegisterCoordinatorCommand, Result<RegisterCoordinatorResponse>>
    {
        public async Task<Result<RegisterCoordinatorResponse>> Handle(
            RegisterCoordinatorCommand command,
            CancellationToken cancellationToken)
        {
            var token = await tokenRepository.GetByTokenAsync(command.Token, cancellationToken);

            // Validate token exists and is the correct type
            if (token is null || token.TokenType != TokenType.CoordinatorInvitation)
                return AuthErrors.InvalidCoordinatorToken;

            if (!token.IsValid)
                return AuthErrors.InvalidCoordinatorToken;

            if (token.IsUsed)
                return AuthErrors.TokenAlreadyUsed;

            var user = await userRepository.GetByIdAsync(token.UserId, cancellationToken);
            if (user is null)
                return AuthErrors.InvalidCoordinatorToken;

            var passwordHash = passwordHasher.Hash(command.Password);

            var refreshToken = tokenService.GenerateRefreshToken();
            var refreshExpiry = tokenService.GetRefreshTokenExpiry();

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // Complete registration — set all provided info
                user.CompleteCoordinatorRegistration(
                    fullName: command.FullName,
                    phone: command.Phone,
                    passwordHash: passwordHash);
                user.SetRefreshToken(refreshToken, refreshExpiry);
                token.MarkAsUsed();

                await userRepository.UpdateAsync(user, cancellationToken);
                await tokenRepository.UpdateAsync(token, cancellationToken);

            }, cancellationToken);

            return Result<RegisterCoordinatorResponse>.Success(
                new RegisterCoordinatorResponse(
                    UserId: user.Id,
                    FullName: user.FullName,
                    Email: user.Email,
                    Role: user.Role.ToString()
                ));
        }
    }

}
