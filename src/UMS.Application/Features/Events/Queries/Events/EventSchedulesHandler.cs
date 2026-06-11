using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Contracts.Events;
using UMS.Domain.Common;
using UMS.Domain.Entities;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Events.Queries.GetEvents
{
    public sealed record GetEventSchedulesQuery(string EventId)
           : IRequest<Result<EventSchedulesResponse>>;

    public sealed class GetEventSchedulesQueryHandler(
        IEventsApiClient eventsApiClient,
        IScheduleAssignmentRepository assignmentRepository
    ) : IRequestHandler<GetEventSchedulesQuery, Result<EventSchedulesResponse>>
    {
        public async Task<Result<EventSchedulesResponse>> Handle(
            GetEventSchedulesQuery query,
            CancellationToken cancellationToken)
        {
            try
            {
                var detail = await eventsApiClient
                    .GetEventByIdAsync(query.EventId, cancellationToken);

                if (detail is null)
                    return ScheduleErrors.EventNotFound;

                var assignmentMap = new Dictionary<string, ScheduleAssignment>();

                foreach (var schedule in detail.Schedules)
                {
                    var assignment = await assignmentRepository
                        .GetByScheduleIdAsync(schedule.EventScheduleId, cancellationToken);

                    if (assignment is not null)
                        assignmentMap[assignment.ExternalScheduleId] = assignment;
                }

                var schedules = detail.Schedules.Select(s =>
                {
                    assignmentMap.TryGetValue(s.EventScheduleId, out var assignment);

                    return new ScheduleSummary(
                        EventScheduleId: s.EventScheduleId,
                        StartDate: s.StartDate,
                        EndDate: s.EndDate,
                        Venue: s.Venue,
                        City: s.City,
                        Country: s.Country,
                        IsOngoing: s.IsOngoing,
                        IsUpcoming: s.IsUpcoming,
                        IsCompleted: s.IsCompleted,
                        AssignedCoordinatorId: assignment?.CoordinatorId,
                        AssignedCoordinatorName: assignment?.Coordinator.FullName,
                        AssignedAt: assignment?.AssignedAt
                    );
                }).ToList();

                return Result<EventSchedulesResponse>.Success(new EventSchedulesResponse(
                    EventId: detail.Event.EventId,
                    EventName: detail.Event.EventName,
                    Schedules: schedules
                ));
            }
            catch
            {
                return ScheduleErrors.ExternalApiFailed;
            }
        }
    }

}