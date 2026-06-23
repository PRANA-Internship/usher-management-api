using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Domain.Common;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Coordinator.Commands.UpdateProfile;

public sealed class UpdateCoordinatorProfileCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher
) : IRequestHandler<UpdateCoordinatorProfileCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        UpdateCoordinatorProfileCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            return UserErrors.NotFound;

        if (!string.IsNullOrWhiteSpace(command.CurrentPassword) && !string.IsNullOrWhiteSpace(command.NewPassword))
        {
            if (!passwordHasher.Verify(command.CurrentPassword, user.PasswordHash!))
                return new Error("AUTH_010", "Current password is incorrect.");

            user.UpdatePassword(passwordHasher.Hash(command.NewPassword));
        }

        try
        {
            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (command.FullName is not null)
                    user.UpdateFullName(command.FullName);

                if (command.Phone is not null)
                    user.UpdatePhone(command.Phone);

                await userRepository.UpdateAsync(user, cancellationToken);
            }, cancellationToken);
        }
        catch
        {
            return new Error("USER_001", "Failed to update profile.");
        }

        return Result<bool>.Success(true);
    }
}