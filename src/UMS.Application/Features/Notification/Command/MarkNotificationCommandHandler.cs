using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Domain.Common;

namespace UMS.Application.Features.Notification.Command
{
    public sealed record MarkNotificationsCommand(
    Guid UserId,
    Guid? NotificationId
) : IRequest<Result<bool>>;

    public sealed class MarkNotificationsCommandHandler(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<MarkNotificationsCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(
            MarkNotificationsCommand command,
            CancellationToken cancellationToken)
        {
            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (command.NotificationId.HasValue)
                {
                    var notification = await notificationRepository
                        .GetByIdAsync(command.NotificationId.Value, cancellationToken);

                    if (notification is not null &&
                        notification.RecipientId == command.UserId)
                    {
                        notification.MarkAsRead();
                        await notificationRepository
                            .UpdateAsync(notification, cancellationToken);
                    }
                }
                else
                {
                    await notificationRepository
                        .MarkAllAsReadAsync(command.UserId, cancellationToken);
                }
            }, cancellationToken);

            return Result<bool>.Success(true);
        }
    }

}
