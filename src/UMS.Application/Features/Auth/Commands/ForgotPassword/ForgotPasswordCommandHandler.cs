using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Domain.Common;
using UMS.Domain.Entities;
using UMS.Domain.Enums;

namespace UMS.Application.Features.Auth.Commands.ForgotPassword
{
    public sealed class ForgotPasswordCommandHandler(
    IUserRepository userRepository,
    IEmailVerificationTokenRepository tokenRepository,
    IEmailService emailService,
    IUnitOfWork unitOfWork
) : IRequestHandler<ForgotPasswordCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(
            ForgotPasswordCommand command,
            CancellationToken cancellationToken)
        {
            var email = command.Email.Trim().ToLowerInvariant();
            var user = await userRepository.GetByEmailAsync(email, cancellationToken);

            if (user is null)
                return Result<bool>.Success(true);

            var resetToken = EmailVerificationToken.Create(
                userId: user.Id,
                tokenType: TokenType.PasswordReset,
                validFor: TimeSpan.FromHours(1));

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await tokenRepository.AddAsync(resetToken, cancellationToken);
            }, cancellationToken);

            try
            {
                await emailService.SendPasswordResetAsync(
                    toEmail: user.Email,
                    fullName: user.FullName,
                    token: resetToken.Token,
                    ct: cancellationToken);
            }
            catch
            {

            }

            return Result<bool>.Success(true);
        }
    }
}
