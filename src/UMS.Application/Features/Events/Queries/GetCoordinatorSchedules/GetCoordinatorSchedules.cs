using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Events;
using UMS.Domain.Common;
using UMS.Domain.Enums;

namespace UMS.Application.Features.Events.Queries.GetCoordinatorSchedules
{
    public sealed record GetCoordinatorSchedulesQuery(Guid CoordinatorId)
        : IRequest<Result<GetCoordinatorSchedulesResponse>>;
    public sealed class GetCoordinatorSchedulesQueryHandler(
    IScheduleAssignmentRepository assignmentRepository,
    IEventsApiClient eventsApiClient
) : IRequestHandler<GetCoordinatorSchedulesQuery, Result<GetCoordinatorSchedulesResponse>>
    {
        public async Task<Result<GetCoordinatorSchedulesResponse>> Handle(
            GetCoordinatorSchedulesQuery query,
            CancellationToken cancellationToken)
        {
            var assignments = await assignmentRepository.GetByCoordinatorIdAsync(
                query.CoordinatorId, cancellationToken);

            if (assignments.Count == 0)
                return Result<GetCoordinatorSchedulesResponse>.Success(
                    new GetCoordinatorSchedulesResponse([]));

            var uniqueEventIds = assignments
                .Select(a => a.ExternalEventId)
                .Distinct()
                .ToList();

            // Fetch all events in parallel
            var eventDetails = await Task.WhenAll(
                uniqueEventIds.Select(id =>
                    eventsApiClient.GetEventByIdAsync(id, cancellationToken)));



            var scheduleMap = eventDetails
                .Where(e => e is not null)
                .SelectMany(e => e!.Schedules.Select(s => new
                {
                    s.EventScheduleId,
                    e.Event.EventName,
                    Schedule = s
                }))
                .ToDictionary(x => x.EventScheduleId);

            var summaries = assignments
                .Where(a => scheduleMap.ContainsKey(a.ExternalScheduleId))
                .Select(a =>
                {
                    var info = scheduleMap[a.ExternalScheduleId];

                    return new CoordinatorScheduleSummary(
                        ExternalScheduleId: a.ExternalScheduleId,
                        ExternalEventId: a.ExternalEventId,
                        EventName: info.EventName,
                        StartDate: info.Schedule.StartDate,
                        EndDate: info.Schedule.EndDate,
                        Venue: info.Schedule.Venue,
                        City: info.Schedule.City,
                        Country: info.Schedule.Country,
                        IsOngoing: info.Schedule.IsOngoing,
                        IsUpcoming: info.Schedule.IsUpcoming,
                        IsCompleted: info.Schedule.IsCompleted
                    );
                }).ToList();

            return Result<GetCoordinatorSchedulesResponse>.Success(
                new GetCoordinatorSchedulesResponse(summaries));
        }
    }
}
