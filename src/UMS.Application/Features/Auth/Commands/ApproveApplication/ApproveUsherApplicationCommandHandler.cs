

using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Auth;
using UMS.Domain.Common;
using UMS.Domain.Entities;
using UMS.Domain.Enums;
using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Auth.Commands.ApproveApplication
{

    public sealed class ApproveUsherApplicationCommandHandler(
        IUsherRepository usherRepository,
        IUserRepository userRepository,
        IEmailVerificationTokenRepository tokenRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<ApproveUsherApplicationCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(
            ApproveUsherApplicationCommand command,
            CancellationToken cancellationToken)
        {
            var usher = await usherRepository.GetByIdAsync(command.UsherId, cancellationToken);
            if (usher is null)
                return UsherErrors.NotFound;

            if (usher.ApprovalStatus == ApprovalStatus.APPROVED)
                return UsherErrors.AlreadyApproved;

            var user = await userRepository.GetByIdAsync(usher.UserId, cancellationToken);
            if (user is null)
                return UsherErrors.NotFound;

            // Generate password setup token
            var verificationToken = EmailVerificationToken.Create(
                userId: user.Id,
                tokenType: TokenType.EmailVerification,
                validFor: TimeSpan.FromHours(48));

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                usher.ApproveUsher(command.AdminId);
                await usherRepository.UpdateAsync(usher, cancellationToken);
                await tokenRepository.AddAsync(verificationToken, cancellationToken);
            }, cancellationToken);

            // Send password setup email outside transaction
            try
            {
                await emailService.SendPasswordSetupAsync(
                    toEmail: user.Email,
                    fullName: user.FullName,
                    token: verificationToken.Token,
                    ct: cancellationToken);
            }
            catch
            {
            }

            return Result<Guid>.Success(usher.Id);
        }
    }

}
