using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Coordinator;
using UMS.Domain.Common;

namespace UMS.Application.Features.Coordinator.Queries.DashboardAnalytics
{
    public sealed record DashboardAnalyticsQuery(Guid CoordinatorId)
        : IRequest<Result<CoordinatorDashboardAnalyticsResponse>>;

    public sealed class DashboardAnalyticsQueryHandler(
        IScheduleAssignmentRepository assignmentRepository,
        IUsherScheduleApplicationRepository applicationRepository,
        IUsherInvitationRepository invitationRepository,
        IEventsApiClient eventsApiClient
    ) : IRequestHandler<DashboardAnalyticsQuery, Result<CoordinatorDashboardAnalyticsResponse>>
    {
        public async Task<Result<CoordinatorDashboardAnalyticsResponse>> Handle(
            DashboardAnalyticsQuery query,
            CancellationToken cancellationToken)
        {
            var assignments = await assignmentRepository.GetByCoordinatorIdAsync(
                query.CoordinatorId, cancellationToken);

            if (assignments.Count == 0)
            {
                return Result<CoordinatorDashboardAnalyticsResponse>.Success(
                    new CoordinatorDashboardAnalyticsResponse(0, 0));
            }

            var scheduleIds = assignments
                .Select(a => a.ExternalScheduleId)
                .Distinct()
                .ToList();

            var totalConfirmedTasks = scheduleIds.Select(async id =>
            {
                var appCount = await applicationRepository.CountApprovedByScheduleAsync(id, cancellationToken);
                var inviteCount = await invitationRepository.CountAcceptedByScheduleAsync(id, cancellationToken);
                return appCount + inviteCount;
            });

            var confirmedCounts = await Task.WhenAll(totalConfirmedTasks);
            var totalUshersConfirmed = confirmedCounts.Sum();

            var uniqueEventIds = assignments
                .Select(a => a.ExternalEventId)
                .ToHashSet();

            int activeEventsCount;
            try
            {
                var publicEvents = await eventsApiClient.GetEventsAsync(cancellationToken);
                activeEventsCount = publicEvents.Count(e => uniqueEventIds.Contains(e.EventId));
            }
            catch
            {
                activeEventsCount = 0;
            }

            return Result<CoordinatorDashboardAnalyticsResponse>.Success(
                new CoordinatorDashboardAnalyticsResponse(totalUshersConfirmed, activeEventsCount));
        }
    }
}
