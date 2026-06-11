using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Application.Common.Models;
using UMS.Contracts.Coordinator;
using UMS.Domain.Common;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Coordinator.Queries.GetAvailableUshersQuery
{
    public sealed class GetAvailableUshersQueryHandler(
      IUsherRepository usherRepository,
      IUsherScheduleApplicationRepository applicationRepository,
      IUsherInvitationRepository invitationRepository,
      IScheduleAssignmentRepository assignmentRepository,
      IEventsApiClient eventsApiClient
  ) : IRequestHandler<AvailableUshersQuery, Result<PagedAvailableUshersResponse>>
    {
        public async Task<Result<PagedAvailableUshersResponse>> Handle(
            AvailableUshersQuery query,
            CancellationToken cancellationToken)
        {
            var assignment = await assignmentRepository
                .GetByScheduleIdAsync(query.ExternalScheduleId, cancellationToken);

            if (assignment is null)
                return ScheduleErrors.ScheduleNotFound;

            if (assignment.CoordinatorId != query.CoordinatorId)
                return InvitationErrors.NotYourSchedule;

            ScheduleDto? schedule;
            try
            {
                schedule = await eventsApiClient.GetScheduleByIdAsync(
                    query.ExternalEventId,
                    query.ExternalScheduleId,
                    cancellationToken);
            }
            catch
            {
                return ScheduleErrors.ExternalApiFailed;
            }

            if (schedule is null)
                return ScheduleErrors.ScheduleNotFound;

            var startDate = DateOnly.Parse(schedule.StartDate);
            var endDate = DateOnly.Parse(schedule.EndDate);

            var conflictedAppIds = await applicationRepository
                .GetConflictedUsherIdsAsync(startDate, endDate, cancellationToken);

            var conflictedInviteIds = await invitationRepository
                .GetConflictedUsherIdsAsync(startDate, endDate, cancellationToken);

            var alreadyAppliedIds = await applicationRepository
                .GetUsherIdsByScheduleAsync(
                    query.ExternalScheduleId, cancellationToken);

            var alreadyInvitedIds = await invitationRepository
                .GetUsherIdsByScheduleAsync(
                    query.ExternalScheduleId, cancellationToken);

            var excludedIds = conflictedAppIds
                .Concat(conflictedInviteIds)
                .Concat(alreadyAppliedIds)
                .Concat(alreadyInvitedIds)
                .ToHashSet();

            var (ushers, totalCount) = await usherRepository
                .GetAvailablePagedAsync(excludedIds, query.Page, query.Size, cancellationToken);

            var totalPages = (int)Math.Ceiling(totalCount / (double)query.Size);

            var items = ushers.Select(u => new AvailableUsherItem(
                UsherId: u.UserId,
                FullName: u.User!.FullName,
                Email: u.User.Email,
                Phone: u.User.Phone,
                City: u.City,
                Languages: u.Languages.Select(l => l.ToString()).ToList(),
                Sectors: u.Sector.Select(s => s.ToString()).ToList()
            )).ToList();

            return Result<PagedAvailableUshersResponse>.Success(
                new PagedAvailableUshersResponse(
                    Items: items,
                    TotalCount: totalCount,
                    Page: query.Page,
                    Size: query.Size,
                    TotalPages: totalPages
                ));
        }
    }
}