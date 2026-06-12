using System;
using System.Collections.Generic;
using System.Text;

using UMS.Domain.Enums;
namespace UMS.Domain.Entities
{


    public class Notification : BaseEntity
    {
        public Guid RecipientId { get; private set; }
        public NotificationType Type { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string Message { get; private set; } = string.Empty;
        public bool IsRead { get; private set; } = false;
        public DateTimeOffset? ReadAt { get; private set; }
        public string? Payload { get; private set; }
        private Notification() { }

        public static Notification Create(
            Guid recipientId,
            NotificationType type,
            string title,
            string message,
            string? payload = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);

            return new Notification
            {
                Id = Guid.NewGuid(),
                RecipientId = recipientId,
                Type = type,
                Title = title,
                Message = message,
                IsRead = false,
                Payload = payload,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        public void MarkAsRead()
        {
            if (IsRead) return;
            IsRead = true;
            ReadAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}