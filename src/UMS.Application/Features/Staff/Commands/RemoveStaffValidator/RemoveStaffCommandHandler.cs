using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Domain.Common;
using UMS.Domain.Enums;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Staff.Commands.RemoveStaffValidator
{

    public sealed class RemoveStaffCommandHandler(
        IUserRepository userRepository,
        IEmailVerificationTokenRepository tokenRepository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<RemoveStaffCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(
            RemoveStaffCommand command,
            CancellationToken cancellationToken)
        {
            if (command.AdminId == command.StaffUserId)
                return StaffErrors.CannotRemoveSelf;

            var staff = await userRepository.GetByIdAsync(command.StaffUserId, cancellationToken);

            if (staff is null)
                return StaffErrors.StaffNotFound;

            if (staff.Role != UserRole.ADMIN && staff.Role != UserRole.EVENT_COORDINATOR)
                return StaffErrors.CannotRemoveUsher;
            if (staff.CreatedByAdminId != command.AdminId)
                return StaffErrors.NotYourStaff;


            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await tokenRepository.DeleteByUserIdAsync(staff.Id, cancellationToken);
                await userRepository.DeleteAsync(staff, cancellationToken);
            }, cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}