using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

using UMS.Application.Common;
using UMS.Application.Common.Interfaces;
using UMS.Domain.Common;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Coordinator.Commands.ReviewApplication
{
    public sealed class ReviewApplicationCommandHandler(
      IUsherScheduleApplicationRepository applicationRepository,
      IScheduleAssignmentRepository assignmentRepository,
      IUnitOfWork unitOfWork,
      INotificationService notificationService,
      ICacheService cache
  ) : IRequestHandler<ReviewApplicationCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(
            ReviewApplicationCommand command,
            CancellationToken cancellationToken)
        {
            var application = await applicationRepository
                .GetByIdAsync(command.ApplicationId, cancellationToken);

            if (application is null)
                return UsherScheduleErrors.ApplicationNotFound;

            var assignment = await assignmentRepository
                .GetByScheduleIdAsync(
                    application.ExternalScheduleId, cancellationToken);

            if (assignment is null || assignment.CoordinatorId != command.CoordinatorId)
                return InvitationErrors.NotYourSchedule;

            var result = command.Approve
                ? application.Approve(command.CoordinatorId)
                : application.Reject(command.CoordinatorId);

            if (result != Error.None)
                return result;

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await applicationRepository.UpdateAsync(application, cancellationToken);
            }, cancellationToken);
            try
            {
                if (command.Approve)
                {
                    await cache.RemoveAsync(CacheKeys.CoordinatorAnalytics(command.CoordinatorId), cancellationToken);

                    await notificationService
                        .NotifyUsherApplicationApprovedAsync(
                            userId: application.Usher.User!.Id,
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