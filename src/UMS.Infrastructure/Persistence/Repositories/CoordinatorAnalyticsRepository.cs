using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using UMS.Application.Common.Interfaces;
using UMS.Contracts.Coordinator;
using UMS.Domain.Enums;
using UMS.Infrastructure.Persistance.Context;

namespace UMS.Infrastructure.Persistence.Repositories
{
    public sealed class CoordinatorAnalyticsRepository(AppDbContext db)
        : ICoordinatorAnalyticsRepository
    {
        public async Task<CoordinatorDashboardAnalyticsResponse> GetCoordinatorAnalyticsAsync(
            Guid coordinatorId, CancellationToken ct = default)
        {
            var activeEvents = await db.ScheduleAssignments
                .Where(sa => sa.CoordinatorId == coordinatorId)
                .Select(sa => sa.ExternalEventId)
                .Distinct()
                .CountAsync(ct);

            var invitationUsherIdsQuery = db.UsherInvitations
                .Where(i => i.Status == InvitationStatus.ACCEPTED &&
                            db.ScheduleAssignments.Any(sa => sa.CoordinatorId == coordinatorId && sa.ExternalScheduleId == i.ExternalScheduleId))
                .Select(i => i.UsherId);

            var applicationUsherIdsQuery = db.UsherScheduleApplications
                .Where(a => a.Status == InvitationStatus.ACCEPTED &&
                            db.ScheduleAssignments.Any(sa => sa.CoordinatorId == coordinatorId && sa.ExternalScheduleId == a.ExternalScheduleId))
                .Select(a => a.UsherId);

            var totalUshersConfirmed = await invitationUsherIdsQuery
                .Union(applicationUsherIdsQuery)
                .Distinct()
                .CountAsync(ct);

            return new CoordinatorDashboardAnalyticsResponse(
                TotalUshersConfirmed: totalUshersConfirmed,
                ActiveEvents: activeEvents
            );
        }
    }
}