using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Domain.Common;
using UMS.Domain.Enums;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Ushers.Command.RespondToInvitaion
{

    public sealed class RespondToInvitationCommandHandler(
        IUsherRepository usherRepository,
        IUsherInvitationRepository invitationRepository,
        IUsherAvailablityService availabilityService,
        IUnitOfWork unitOfWork,
        IScheduleAssignmentRepository assignmentRepository,
        INotificationService notificationService
    ) : IRequestHandler<RespondToInvitationCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(
            RespondToInvitationCommand command,
            CancellationToken cancellationToken)
        {
            var invitation = await invitationRepository.GetByIdAsync(
                command.InvitationId, cancellationToken);

            if (invitation is null)
                return UsherScheduleErrors.InvitationNotFound;

            var usher = await usherRepository.GetByIdAsync(
                command.UsherId, cancellationToken);

            if (usher is null || invitation.UsherId != usher.Id)
                return UsherScheduleErrors.NotYourInvitation;

            if (invitation.Status != InvitationStatus.PENDING)
                return UsherScheduleErrors.AlreadyResponded;

            if (command.Accept)
            {
                var isAvailable = await availabilityService.IsAvailableAsync(
                    usher.Id,
                    invitation.ScheduleStartDate,
                    invitation.ScheduleEndDate,
                    cancellationToken);

                if (!isAvailable)
                    return UsherScheduleErrors.NotAvailable;
            }

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var error = command.Accept
                    ? invitation.Accept()
                    : invitation.Decline();

                if (error != Error.None)
                    throw new InvalidOperationException(error.Description);

                await invitationRepository.UpdateAsync(invitation, cancellationToken);
            }, cancellationToken);
            try
            {
                if (command.Accept)
                {
                    var assignment = await assignmentRepository
                        .GetByScheduleIdAsync(invitation.ExternalScheduleId, cancellationToken);

                    await notificationService
                        .NotifyCoordinatorUsherAcceptedAsync(
                            assignment!.CoordinatorId,
                            usherFullName: usher.User!.FullName,
                            cancellationToken);
                }
            }
            catch (Exception)
            {

            }

            return Result<bool>.Success(true);
        }
    }
}