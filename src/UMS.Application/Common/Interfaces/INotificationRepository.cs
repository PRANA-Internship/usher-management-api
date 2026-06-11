using System;
using System.Collections.Generic;
using System.Text;

using UMS.Domain.Entities;

namespace UMS.Application.Common.Interfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification, CancellationToken ct = default);
        Task AddRangeAsync(IReadOnlyList<Notification> notifications, CancellationToken ct = default);
        Task UpdateAsync(Notification notification, CancellationToken ct = default);

        Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetPagedByRecipientAsync(
            Guid recipientId,
            bool? isRead,
            int page,
            int size,
            CancellationToken ct = default);

        Task<int> GetUnreadCountAsync(
            Guid recipientId, CancellationToken ct = default);

        Task MarkAllAsReadAsync(
            Guid recipientId, CancellationToken ct = default);
    }
}