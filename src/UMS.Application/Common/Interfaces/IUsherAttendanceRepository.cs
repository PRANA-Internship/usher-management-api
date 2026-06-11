using System;
using System.Collections.Generic;
using System.Text;

using UMS.Domain.Entities;
using UMS.Domain.Enums;

namespace UMS.Application.Common.Interfaces
{

    public interface IUsherAttendanceRepository
    {
        Task AddAsync(UsherAttendance attendance, CancellationToken ct = default);
        Task UpdateAsync(UsherAttendance attendance, CancellationToken ct = default);

        Task<UsherAttendance?> GetAsync(
            string scheduleId,
            Guid usherId,
            DateOnly date,
            DayStatus dayStatus,
            CancellationToken ct = default);

        Task<IReadOnlyList<UsherAttendance>> GetByScheduleDateStatusAsync(
            string scheduleId,
            DateOnly date,
            DayStatus dayStatus,
            CancellationToken ct = default);

        Task<IReadOnlyList<UsherAttendance>> GetByUsherAndScheduleAsync(
            Guid usherId,
            string scheduleId,
            CancellationToken ct = default);

        Task<bool> ExistsAsync(
            string scheduleId,
            Guid usherId,
            DateOnly date,
            DayStatus dayStatus,
            CancellationToken ct = default);

        // Bulk create attendance records for all confirmed ushers
        Task AddRangeAsync(
            IReadOnlyList<UsherAttendance> records,
            CancellationToken ct = default);
        Task<IReadOnlyList<UsherAttendance>> GetRawByUsherAsync(
            Guid usherId, CancellationToken ct = default);

    }
}