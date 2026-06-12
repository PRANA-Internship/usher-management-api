using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Notification
{

    public sealed record NotificationDto(
        Guid Id,
        string Type,
        string Title,
        string Message,
        bool IsRead,
        DateTimeOffset CreatedAt,
        string? Payload
    );

    public sealed record UnreadCountDto(int Count);

    public sealed record PagedNotificationResponse(
        IReadOnlyList<NotificationDto> Items,
        int TotalCount,
        int Page,
        int Size,
        int TotalPages,
        int UnreadCount
    );
}