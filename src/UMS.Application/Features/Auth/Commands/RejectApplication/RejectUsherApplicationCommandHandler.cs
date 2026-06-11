using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Domain.Common;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Auth.Commands.RejectApplication
{

    public sealed class RejectUsherApplicationCommandHandler(
        IUsherRepository usherRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<RejectUsherApplicationCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(
            RejectUsherApplicationCommand command,
            CancellationToken cancellationToken)
        {
            var usher = await usherRepository.GetByIdAsync(command.UsherId, cancellationToken);

            if (usher is null)
                return UsherErrors.NotFound;

            var rejectionResult = usher.RejectApplication();

            if (rejectionResult != Error.None)
                return rejectionResult;

            var user = await userRepository.GetByIdAsync(usher.UserId, cancellationToken);

            if (user is null)
                return UsherErrors.NotFound;

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await usherRepository.UpdateAsync(usher, cancellationToken);
            }, cancellationToken);
            try
            {
                await emailService.SendApplicationRejectedAsync(
                    user.Email, user.FullName, cancellationToken);
            }
            catch
            {
            }

            return Result<Guid>.Success(usher.Id);
        }
    }

}