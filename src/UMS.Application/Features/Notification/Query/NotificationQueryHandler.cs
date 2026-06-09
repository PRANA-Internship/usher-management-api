using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Notification;
using UMS.Domain.Common;

namespace UMS.Application.Features.Notification.Query
{
    public sealed class GetMyNotificationsQueryHandler(
     INotificationRepository notificationRepository
 ) : IRequestHandler<NotificationsQuery, Result<PagedNotificationResponse>>
    {
        public async Task<Result<PagedNotificationResponse>> Handle(
            NotificationsQuery query,
            CancellationToken cancellationToken)
        {
            var (items, total) = await notificationRepository
                .GetPagedByRecipientAsync(
                    query.UserId, query.IsRead,
                    query.Page, query.Size, cancellationToken);

            var unreadCount = await notificationRepository
                .GetUnreadCountAsync(query.UserId, cancellationToken);

            var totalPages = (int)Math.Ceiling(total / (double)query.Size);

            var mapped = items.Select(n => new NotificationDto(
                Id: n.Id,
                Type: n.Type.ToString(),
                Title: n.Title,
                Message: n.Message,
                IsRead: n.IsRead,
                CreatedAt: n.CreatedAt,
                Payload: n.Payload
            )).ToList();

            return Result<PagedNotificationResponse>.Success(
                new PagedNotificationResponse(
                    Items: mapped,
                    TotalCount: total,
                    Page: query.Page,
                    Size: query.Size,
                    TotalPages: totalPages,
                    UnreadCount: unreadCount
                ));
        }
    }
}
