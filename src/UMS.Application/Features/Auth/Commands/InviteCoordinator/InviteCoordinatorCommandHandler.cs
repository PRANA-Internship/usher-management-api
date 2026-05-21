using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Domain.Common;
using UMS.Domain.Entities;
using UMS.Domain.Enums;
using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Auth.Commands.InviteCoordinator
{

    public sealed class InviteCoordinatorCommandHandler(
        IUserRepository userRepository,
        IEmailVerificationTokenRepository tokenRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<InviteCoordinatorCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(
     InviteCoordinatorCommand command,
     CancellationToken cancellationToken)
        {
            var email = command.Email.Trim().ToLowerInvariant();

            var existingUser = await userRepository.GetByEmailAsync(email, cancellationToken);

            if (existingUser is not null)
            {
                var isIncompleteInvitation =
                    existingUser.Role == UserRole.GUEST &&
                    !existingUser.EmailVerified &&
                    !existingUser.HasPassword();

                if (!isIncompleteInvitation)
                {
                    return AuthErrors.CoordinatorAlreadyExists;
                }


                var existingToken = await tokenRepository
                    .GetActiveByUserIdAsync(existingUser.Id, TokenType.CoordinatorInvitation, cancellationToken);

                if (existingToken is not null && !existingToken.IsExpired)
                {
                    return AuthErrors.InvitationAlreadyPending;
                }

                await unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    await tokenRepository.DeleteByUserIdAsync(existingUser.Id, cancellationToken);
                    await userRepository.DeleteAsync(existingUser, cancellationToken);
                }, cancellationToken);
            }
            var placeholderUser = User.CreateInvitedCoordinator(email);

            var invitationToken = EmailVerificationToken.Create(
                userId: placeholderUser.Id,
                tokenType: TokenType.CoordinatorInvitation,
                validFor: TimeSpan.FromHours(48));

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await userRepository.AddAsync(placeholderUser, cancellationToken);
                await tokenRepository.AddAsync(invitationToken, cancellationToken);
            }, cancellationToken);

            try
            {
                await emailService.SendCoordinatorInvitationAsync(
                    toEmail: email,
                    token: invitationToken.Token,
                    ct: cancellationToken);
            }
            catch { }

            return Result<bool>.Success(true);
        }

    }
}
