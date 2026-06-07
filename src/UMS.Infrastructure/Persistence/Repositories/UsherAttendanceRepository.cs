using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Domain.Entities;
using UMS.Domain.Enums;
using UMS.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace UMS.Infrastructure.Persistence.Repositories
{
    public sealed class UsherAttendanceRepository(AppDbContext db)
    : IUsherAttendanceRepository
    {
        public async Task AddAsync(
            UsherAttendance attendance, CancellationToken ct = default) =>
            await db.UsherAttendances.AddAsync(attendance, ct);

        public Task UpdateAsync(
            UsherAttendance attendance, CancellationToken ct = default)
        {
            db.UsherAttendances.Update(attendance);
            return Task.CompletedTask;
        }

        public Task<UsherAttendance?> GetAsync(
            string scheduleId,
            Guid usherId,
            DateOnly date,
            DayStatus dayStatus,
            CancellationToken ct = default) =>
            db.UsherAttendances
              .Include(a => a.Usher).ThenInclude(u => u.User)
              .FirstOrDefaultAsync(a =>
                  a.ExternalScheduleId == scheduleId &&
                  a.UsherId == usherId &&
                  a.AttendanceDate == date &&
                  a.DayStatus == dayStatus, ct);

        public async Task<IReadOnlyList<UsherAttendance>> GetByScheduleDateStatusAsync(
            string scheduleId,
            DateOnly date,
            DayStatus dayStatus,
            CancellationToken ct = default) =>
            await db.UsherAttendances
                .Include(a => a.Usher).ThenInclude(u => u.User)
                .Where(a =>
                    a.ExternalScheduleId == scheduleId &&
                    a.AttendanceDate == date &&
                    a.DayStatus == dayStatus)
                .OrderBy(a => a.Usher.User!.FullName)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<UsherAttendance>> GetByUsherAndScheduleAsync(
            Guid usherId,
            string scheduleId,
            CancellationToken ct = default) =>
            await db.UsherAttendances
                .Where(a =>
                    a.UsherId == usherId &&
                    a.ExternalScheduleId == scheduleId)
                .OrderBy(a => a.AttendanceDate)
                .ThenBy(a => a.DayStatus)
                .ToListAsync(ct);

        public Task<bool> ExistsAsync(
            string scheduleId,
            Guid usherId,
            DateOnly date,
            DayStatus dayStatus,
            CancellationToken ct = default) =>
            db.UsherAttendances.AnyAsync(a =>
                a.ExternalScheduleId == scheduleId &&
                a.UsherId == usherId &&
                a.AttendanceDate == date &&
                a.DayStatus == dayStatus, ct);

        public async Task AddRangeAsync(
            IReadOnlyList<UsherAttendance> records,
            CancellationToken ct = default) =>
            await db.UsherAttendances.AddRangeAsync(records, ct);
        public async Task<IReadOnlyList<UsherAttendance>> GetRawByUsherAsync(
           Guid usherId, CancellationToken ct = default) =>
           await db.UsherAttendances
                   .Where(a => a.UsherId == usherId)
                   .ToListAsync(ct);
    }

}
