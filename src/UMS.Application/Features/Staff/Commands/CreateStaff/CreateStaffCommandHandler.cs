using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Staff;
using UMS.Domain.Common;
using UMS.Domain.Entities;
using UMS.Domain.Enums;
using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Staff.Commands.CreateStaff
{
    public sealed class CreateStaffCommandHandler(
    IUserRepository userRepository,
    IEmailVerificationTokenRepository tokenRepository,
    IEmailService emailService,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateStaffCommand, Result<CreateStaffResponse>>
    {
        public async Task<Result<CreateStaffResponse>> Handle(
       CreateStaffCommand command,
       CancellationToken cancellationToken)
        {
            var email = command.Email.Trim().ToLowerInvariant();

            if (await userRepository.ExistsByEmailAsync(email, cancellationToken))
                return StaffErrors.EmailAlreadyExists;

            var staff = User.CreateStaff(
                fullName: command.FullName,
                email: email,
                phone: command.Phone,
                role: command.Role,

    createdByAdminId: command.AdminId

                );

            var setupToken = EmailVerificationToken.Create(
                userId: staff.Id,
                tokenType: TokenType.StaffSetup,
                validFor: TimeSpan.FromHours(48));

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await userRepository.AddAsync(staff, cancellationToken);
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

            return Result<CreateStaffResponse>.Success(new CreateStaffResponse(
                UserId: staff.Id,
                FullName: staff.FullName,
                Email: staff.Email,
                Phone: staff.Phone,
                Role: staff.Role.ToString(),
                Status: staff.Status.ToString()
            ));
        }








    }
}
