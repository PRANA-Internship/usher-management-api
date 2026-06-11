using System;
using System.Collections.Generic;
using System.Text;

using UMS.Domain.Common;
using UMS.Domain.Enums;


namespace UMS.Domain.Entities
{

    public class UsherAttendance : BaseEntity
    {
        public string ExternalScheduleId { get; private set; } = string.Empty;
        public string ExternalEventId { get; private set; } = string.Empty;
        public Guid UsherId { get; private set; }
        public Guid MarkedByCoordinatorId { get; private set; }
        public DateOnly AttendanceDate { get; private set; }
        public DayStatus DayStatus { get; private set; }
        public AttendanceStatus Status { get; private set; } = AttendanceStatus.NotMarked;
        public bool IsMarked { get; private set; } = false;
        public DateTimeOffset? MarkedAt { get; private set; }

        public Usher Usher { get; private set; } = null!;

        private UsherAttendance() { }

        public static UsherAttendance Create(
            string externalScheduleId,
            string externalEventId,
            Guid usherId,
            Guid coordinatorId,
            DateOnly attendanceDate,
            DayStatus dayStatus)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(externalScheduleId);
            ArgumentException.ThrowIfNullOrWhiteSpace(externalEventId);

            if (usherId == Guid.Empty) throw new ArgumentException("UsherId is required.");
            if (coordinatorId == Guid.Empty) throw new ArgumentException("CoordinatorId is required.");

            return new UsherAttendance
            {
                Id = Guid.NewGuid(),
                ExternalScheduleId = externalScheduleId,
                ExternalEventId = externalEventId,
                UsherId = usherId,
                MarkedByCoordinatorId = coordinatorId,
                AttendanceDate = attendanceDate,
                DayStatus = dayStatus,
                Status = AttendanceStatus.NotMarked,
                IsMarked = false,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        public Error Mark(AttendanceStatus status, Guid coordinatorId)
        {
            if (IsMarked)
                return new Error("ATT_001", "Attendance already marked for this session.");

            if (status == AttendanceStatus.NotMarked)
                return new Error("ATT_002", "Cannot mark attendance as NotMarked.");

            Status = status;
            IsMarked = true;
            MarkedAt = DateTimeOffset.UtcNow;
            MarkedByCoordinatorId = coordinatorId;
            UpdatedAt = DateTimeOffset.UtcNow;

            return Error.None;
        }

        public Error Update(AttendanceStatus status, Guid coordinatorId)
        {
            if (!IsMarked)
                return new Error("ATT_003", "Attendance has not been marked yet.");

            if (status == AttendanceStatus.NotMarked)
                return new Error("ATT_002", "Cannot set attendance to NotMarked.");

            Status = status;
            MarkedByCoordinatorId = coordinatorId;
            MarkedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;

            return Error.None;
        }

        public int Score => IsMarked ? (int)Status : 0;
    }


}