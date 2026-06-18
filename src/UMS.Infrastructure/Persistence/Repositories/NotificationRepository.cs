using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;

using UMS.Application.Common.Interfaces;
using UMS.Domain.Entities;
using UMS.Infrastructure.Persistance.Context;
namespace UMS.Infrastructure.Persistence.Repositories
{

    public sealed class NotificationRepository(AppDbContext db)
        : INotificationRepository
    {
        public async Task AddAsync(
            Notification notification, CancellationToken ct = default) =>
            await db.Notifications.AddAsync(notification, ct);

        public async Task AddRangeAsync(
            IReadOnlyList<Notification> notifications,
            CancellationToken ct = default) =>
            await db.Notifications.AddRangeAsync(notifications, ct);

        public Task UpdateAsync(
            Notification notification, CancellationToken ct = default)
        {
            db.Notifications.Update(notification);
            return Task.CompletedTask;
        }

        public Task<Notification?> GetByIdAsync(
            Guid id, CancellationToken ct = default) =>
            db.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct);

        public async Task<(IReadOnlyList<Notification> Items, int TotalCount)>
            GetPagedByRecipientAsync(
                Guid recipientId,
                bool? isRead,
                int page,
                int size,
                CancellationToken ct = default)
        {
            var query = db.Notifications
                .Where(n => n.RecipientId == recipientId)
                .AsQueryable();

            if (isRead.HasValue)
                query = query.Where(n => n.IsRead == isRead.Value);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(ct);

            return (items, total);
        }

        public Task<int> GetUnreadCountAsync(
            Guid recipientId, CancellationToken ct = default) =>
            db.Notifications.CountAsync(
                n => n.RecipientId == recipientId && !n.IsRead, ct);

        public async Task MarkAllAsReadAsync(
            Guid recipientId, CancellationToken ct = default) =>
            await db.Notifications
                .Where(n => n.RecipientId == recipientId && !n.IsRead)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(n => n.IsRead, true)
                     .SetProperty(n => n.ReadAt, DateTimeOffset.UtcNow)
                     .SetProperty(n => n.UpdatedAt, DateTimeOffset.UtcNow),
                    ct);
    }

}