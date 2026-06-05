using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Admin.Dashboard;
using UMS.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace UMS.Infrastructure.Persistence.Repositories
{
    public sealed class AttendanceAnalyticsRepository(AppDbContext db)
    : IAttendanceAnalyticsRepository
    {
        public async Task<IReadOnlyList<MonthlyAttendanceData>> GetMonthlyTrendAsync(
            int months,
            CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var fromDate = new DateOnly(now.Year, now.Month, 1)
                .AddMonths(-(months - 1));

            var raw = await db.UsherAttendances
                .Where(a => a.AttendanceDate >= fromDate)
                .GroupBy(a => new
                {
                    a.AttendanceDate.Year,
                    a.AttendanceDate.Month
                })
                .Select(g => new MonthlyAttendanceRaw
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalSessions = g.Count(),
                    TotalMarked = g.Count(a => a.IsMarked),
                    TotalScore = g.Sum(a =>
                        (int)a.Status < 0 ? 0 : (int)a.Status),
                    MaxPossibleScore = g.Count() * 2
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync(ct);

            return raw
                .Select(r => new MonthlyAttendanceData(
                    Year: r.Year,
                    Month: r.Month,
                    TotalSessions: r.TotalSessions,
                    TotalMarked: r.TotalMarked,
                    TotalScore: r.TotalScore,
                    MaxPossibleScore: r.MaxPossibleScore))
                .ToList();
        }
    }


}
