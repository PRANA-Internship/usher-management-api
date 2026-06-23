using System;

using Microsoft.EntityFrameworkCore;

using UMS.Application.Common.Interfaces;
using UMS.Contracts.Usher;
using UMS.Domain.Enums;
using UMS.Infrastructure.Persistance.Context;

namespace UMS.Infrastructure.Persistence.Repositories
{
    public sealed class UsherAnalyticsRepository(AppDbContext db)
        : IUsherAnalyticsRepository
    {
        public async Task<UsherAnalyticsSummary> GetUsherAnalyticsAsync(
            Guid usherId, CancellationToken ct = default)
        {
            var hasReviews = await db.UsherPerformanceReviews
                .AnyAsync(r => r.UsherId == usherId, ct);

            var averageScore = hasReviews
                ? await db.UsherPerformanceReviews
                    .Where(r => r.UsherId == usherId)
                    .AverageAsync(r =>
                        (r.Grooming + r.Professionalism +
                         r.Communication + r.Teamwork + r.OverallScore) / 5.0, ct)
                : 0.0;

            var hasAttendance = await db.UsherAttendances
                .AnyAsync(a => a.UsherId == usherId && a.IsMarked, ct);

            var attendanceRate = hasAttendance
                ? await db.UsherAttendances
                    .Where(a => a.UsherId == usherId && a.IsMarked)
                    .AverageAsync(a =>
                        a.Status == AttendanceStatus.Present ? 1.0 :
                        a.Status == AttendanceStatus.Late ? 0.5 :
                        0.0, ct) * 100
                : 0.0;

            var eventsDone = await CountCompletedEventsAsync(usherId, ct);

            return new UsherAnalyticsSummary(
                AverageScore: Math.Round(averageScore, 2),
                AttendanceRate: Math.Round(attendanceRate, 2),
                EventsDone: eventsDone
            );
        }

        public async Task<int> CountCompletedEventsAsync(
     Guid usherId, CancellationToken ct = default)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var fromApps = await db.UsherScheduleApplications
                .Where(a => a.UsherId == usherId
                         && a.Status == InvitationStatus.ACCEPTED
                         && a.ScheduleEndDate < today)
                .CountAsync(ct);

            var fromInvites = await db.UsherInvitations
                .Where(i => i.UsherId == usherId
                         && i.Status == InvitationStatus.ACCEPTED
                         && i.ScheduleEndDate < today)
                .CountAsync(ct);

            return fromApps + fromInvites;
        }
    }
}