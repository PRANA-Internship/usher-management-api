using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Domain.Common;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Account.Commands.UpdateProfile;

public sealed class UpdateUserProfileCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateUserProfileCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        UpdateUserProfileCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            return UserErrors.NotFound;

        try
        {
            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (command.FullName is not null) user.UpdateFullName(command.FullName);
                if (command.Phone is not null) user.UpdatePhone(command.Phone);
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