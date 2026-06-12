using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Contracts.Staff;
using UMS.Domain.Common;
using UMS.Domain.Entities;
using UMS.Domain.Enums;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Staff.Commands.ResendSetupLink
{
    public sealed class ResendSetupLinkCommandHandler(
     IUserRepository userRepository,
     IEmailVerificationTokenRepository tokenRepository,
     IEmailService emailService,
     IUnitOfWork unitOfWork
 ) : IRequestHandler<ResendSetupLinkCommand, Result<ResendSetupLinkResponse>>
    {
        public async Task<Result<ResendSetupLinkResponse>> Handle(
            ResendSetupLinkCommand command,
            CancellationToken cancellationToken)
        {
            var staff = await userRepository.GetByIdAsync(command.StaffUserId, cancellationToken);

            if (staff is null)
                return StaffErrors.StaffNotFound;

            if (staff.Role != UserRole.ADMIN && staff.Role != UserRole.EVENT_COORDINATOR)
                return StaffErrors.CannotRemoveUsher;

            if (staff.Status == UserStatus.ACTIVE)
                return StaffErrors.AlreadyActive;

            await tokenRepository.InvalidateByUserIdAsync(
                staff.Id, TokenType.StaffSetup, cancellationToken);

            var setupToken = EmailVerificationToken.Create(
                userId: staff.Id,
                tokenType: TokenType.StaffSetup,
                validFor: TimeSpan.FromHours(48));

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await tokenRepository.AddAsync(setupToken, cancellationToken);
            }, cancellationToken);

            try
            {
                await emailService.SendStaffSetupAsync(
                    toEmail: staff.Email,
                    fullName: staff.FullName,
                    role: staff.Role.ToString(),
                    token: setupToken.Token,
                    ct: cancellationToken);
            }
            catch
            {
            }

            return Result<ResendSetupLinkResponse>.Success(new ResendSetupLinkResponse(
                UserId: staff.Id,
                Email: staff.Email,
                Message: "Setup link resent successfully."
            ));
        }
    }

}