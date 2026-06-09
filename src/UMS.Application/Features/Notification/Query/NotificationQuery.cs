using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Contracts.Notification;
using UMS.Domain.Common;

namespace UMS.Application.Features.Notification.Query
{

    public sealed record NotificationsQuery(
        Guid UserId,
        bool? IsRead,
        int Page,
        int Size
    ) : IRequest<Result<PagedNotificationResponse>>;

}
