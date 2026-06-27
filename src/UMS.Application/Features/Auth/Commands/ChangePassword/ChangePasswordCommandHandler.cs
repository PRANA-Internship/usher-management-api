using MediatR;
using UMS.Application.Common.Interfaces;
using UMS.Domain.Common;
using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Auth.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork
) : IRequestHandler<ChangePasswordCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            return UserErrors.NotFound;

        if (!passwordHasher.Verify(command.CurrentPassword, user.PasswordHash!))
            return new Error("AUTH_010", "Current password is incorrect.");

        user.UpdatePassword(passwordHasher.Hash(command.NewPassword));

        try
        {
            await unitOfWork.ExecuteInTransactionAsync(async () =>
                await userRepository.UpdateAsync(user, cancellationToken),
                cancellationToken);
        }
        catch
        {
            return new Error("AUTH_012", "Failed to update password.");
        }

        return Result<bool>.Success(true);
    }
}