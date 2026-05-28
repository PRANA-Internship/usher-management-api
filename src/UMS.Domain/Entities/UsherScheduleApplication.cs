using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using UMS.Domain.Enums;
using UMS.Domain.Common;
namespace UMS.Domain.Entities
{
    public class UsherScheduleApplication : BaseEntity
    {
        public string ExternalScheduleId { get; private set; } = null!;
        public string ExternalEventId { get; private set; } = null!;
        public Guid UsherId { get; private set; }
        public InvitationStatus Status { get; private set; } = InvitationStatus.PENDING;
        public Guid? ReviewedBy { get; private set; }
        public DateTimeOffset? ReviewedAt { get; private set; }
        public DateOnly ScheduleStartDate { get; private set; }
        public DateOnly ScheduleEndDate { get; private set; }

        public Usher Usher { get; private set; } = null!;

        private UsherScheduleApplication() { }

        public static UsherScheduleApplication Create(
            string externalScheduleId,
            string externalEventId,
            Guid usherId,
            DateOnly startDate,
            DateOnly endDate)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(externalScheduleId);
            ArgumentException.ThrowIfNullOrWhiteSpace(externalEventId);
            if (usherId == Guid.Empty) throw new ArgumentException("UsherId is required.");
            if (endDate < startDate) throw new ArgumentException("End date must be after start date.");

            return new UsherScheduleApplication
            {
                Id = Guid.NewGuid(),
                ExternalScheduleId = externalScheduleId,
                ExternalEventId = externalEventId,
                UsherId = usherId,
                Status = InvitationStatus.PENDING,
                ScheduleStartDate = startDate,
                ScheduleEndDate = endDate,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        public Error Approve(Guid reviewedBy)
        {
            if (Status != InvitationStatus.PENDING)
                return new Error("APP_001", "Only pending applications can be approved.");

            Status = InvitationStatus.ACCEPTED;
            ReviewedBy = reviewedBy;
            ReviewedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;

            return Error.None;
        }

        public Error Reject(Guid reviewedBy)
        {
            if (Status != InvitationStatus.PENDING)
                return new Error("APP_002", "Only pending applications can be rejected.");

            Status = InvitationStatus.DECLINED;
            ReviewedBy = reviewedBy;
            ReviewedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;

            return Error.None;
        }

    }

}
