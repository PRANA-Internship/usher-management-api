using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using UMS.Application.Common.Interfaces;
using UMS.Contracts.Coordinator.Dashboard;

namespace UMS.Infrastructure.Persistence.Repositories
{
    public sealed class CoordinatorAnalyticsRepository(
        IScheduleAssignmentRepository scheduleAssignmentRepository,
        IUsherScheduleApplicationRepository usherScheduleApplicationRepository,
        IUsherInvitationRepository usherInvitationRepository
    ) : ICoordinatorAnalyticsRepository
    {
        public async Task<CoordinatorDashboardResponse> GetCoordinatorDashboardAnalyticsAsync(Guid coordinatorId, CancellationToken ct = default)
        {
            var assignments = await scheduleAssignmentRepository.GetByCoordinatorIdAsync(coordinatorId, ct);
            var confirmedUsherIds = new HashSet<Guid>();
            foreach (var assignment in assignments)
            {
                var appUsherIds = await usherScheduleApplicationRepository.GetAcceptedUsherIdsByScheduleAsync(assignment.ExternalScheduleId, ct);
                foreach (var id in appUsherIds)
                {
                    confirmedUsherIds.Add(id);
                }

                var inviteUsherIds = await usherInvitationRepository.GetAcceptedUsherIdsByScheduleAsync(assignment.ExternalScheduleId, ct);
                foreach (var id in inviteUsherIds)
                {
                    confirmedUsherIds.Add(id);
                }
            }
            var totalUshersConfirmed = confirmedUsherIds.Count;
            var activeEvents = assignments.Select(a => a.ExternalEventId).Distinct().Count();
            return new CoordinatorDashboardResponse(totalUshersConfirmed, activeEvents);
        }
    }
}