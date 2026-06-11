using System;
using System.Collections.Generic;
using System.Text;

using UMS.Domain.Common;
using UMS.Domain.Enums;

namespace UMS.Domain.Entities
{

    public class UsherInvitation : BaseEntity
    {
        public string ExternalScheduleId { get; private set; } = string.Empty;
        public string ExternalEventId { get; private set; } = string.Empty;
        public Guid UsherId { get; private set; }
        public Guid InvitedByCoordinatorId { get; private set; }
        public InvitationStatus Status { get; private set; } = InvitationStatus.PENDING;
        public DateTimeOffset? RespondedAt { get; private set; }
        public DateOnly ScheduleStartDate { get; private set; }
        public DateOnly ScheduleEndDate { get; private set; }

        public Usher Usher { get; private set; } = null!;
        public User InvitedByCoordinator { get; private set; } = null!;

        private UsherInvitation() { }

        public static UsherInvitation Create(
            string externalScheduleId,
            string externalEventId,
            Guid usherId,
            Guid coordinatorId,
            DateOnly startDate,
            DateOnly endDate)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(externalScheduleId);
            ArgumentException.ThrowIfNullOrWhiteSpace(externalEventId);

            if (usherId == Guid.Empty) throw new ArgumentException("UsherId is required.");
            if (coordinatorId == Guid.Empty) throw new ArgumentException("CoordinatorId is required.");
            if (endDate < startDate) throw new ArgumentException("End date must be after start date.");

            return new UsherInvitation
            {
                Id = Guid.NewGuid(),
                ExternalScheduleId = externalScheduleId,
                ExternalEventId = externalEventId,
                UsherId = usherId,
                InvitedByCoordinatorId = coordinatorId,
                Status = InvitationStatus.PENDING,
                ScheduleStartDate = startDate,
                ScheduleEndDate = endDate,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        public Error Accept()
        {
            if (Status != InvitationStatus.PENDING)
                return new Error("INVITE_001", "Only pending invitations can be accepted.");

            Status = InvitationStatus.ACCEPTED;
            RespondedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;

            return Error.None;
        }

        public Error Decline()
        {
            if (Status != InvitationStatus.PENDING)
                return new Error("INVITE_002", "Only pending invitations can be declined.");

            Status = InvitationStatus.DECLINED;
            RespondedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;

            return Error.None;
        }
    }
}