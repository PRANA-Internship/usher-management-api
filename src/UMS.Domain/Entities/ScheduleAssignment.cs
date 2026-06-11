using System;
using System.Collections.Generic;
using System.Text;

using UMS.Domain.Common;

namespace UMS.Domain.Entities
{

    public class ScheduleAssignment : BaseEntity
    {
        public string ExternalScheduleId { get; private set; } = string.Empty;
        public string ExternalEventId { get; private set; } = string.Empty;
        public Guid CoordinatorId { get; private set; }
        public Guid AssignedByAdminId { get; private set; }
        public DateTimeOffset AssignedAt { get; private set; }

        public User Coordinator { get; private set; } = null!;
        public User AssignedBy { get; private set; } = null!;

        private ScheduleAssignment() { }

        public static ScheduleAssignment Create(
            string externalScheduleId,
            string externalEventId,
            Guid coordinatorId,
            Guid assignedByAdminId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(externalScheduleId);
            ArgumentException.ThrowIfNullOrWhiteSpace(externalEventId);

            if (coordinatorId == Guid.Empty) throw new ArgumentException("CoordinatorId is required.");
            if (assignedByAdminId == Guid.Empty) throw new ArgumentException("AdminId is required.");

            return new ScheduleAssignment
            {
                Id = Guid.NewGuid(),
                ExternalScheduleId = externalScheduleId,
                ExternalEventId = externalEventId,
                CoordinatorId = coordinatorId,
                AssignedByAdminId = assignedByAdminId,
                AssignedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        public Error Reassign(Guid newCoordinatorId, Guid adminId)
        {
            if (newCoordinatorId == CoordinatorId)
                return new Error("SCHEDULE_007", "This coordinator is already assigned.");

            CoordinatorId = newCoordinatorId;
            AssignedByAdminId = adminId;
            AssignedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;

            return Error.None;
        }
    }

}