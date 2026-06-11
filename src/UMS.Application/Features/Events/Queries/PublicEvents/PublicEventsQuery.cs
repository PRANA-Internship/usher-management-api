using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Contracts.Events;
using UMS.Domain.Common;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Events.Queries.GetPublicEvents
{
    public sealed record PublicEventsQuery : IRequest<Result<IReadOnlyList<PublicEventResponse>>>;

    public sealed class GetPublicEventsQueryHandler(
        IEventsApiClient eventsApiClient,
        IScheduleAssignmentRepository assignmentRepository
    ) : IRequestHandler<PublicEventsQuery, Result<IReadOnlyList<PublicEventResponse>>>
    {
        public async Task<Result<IReadOnlyList<PublicEventResponse>>> Handle(
            PublicEventsQuery query,
            CancellationToken cancellationToken)
        {
            try
            {
                var allEvents = await eventsApiClient.GetEventsAsync(cancellationToken);

                var allAssignments = await assignmentRepository
                    .GetAllAsync(cancellationToken);

                var assignedScheduleIds = allAssignments
                    .Select(a => a.ExternalScheduleId)
                    .ToHashSet();

                var assignmentsByEvent = allAssignments
                    .GroupBy(a => a.ExternalEventId)
                    .ToDictionary(g => g.Key, g => g.ToList());
                var result = new List<PublicEventResponse>();

                foreach (var evt in allEvents)
                {
                    if (!assignmentsByEvent.ContainsKey(evt.EventId))
                        continue;

                    var detail = await eventsApiClient
                        .GetEventByIdAsync(evt.EventId, cancellationToken);

                    if (detail is null) continue;

                    var assignedSchedules = detail.Schedules
                        .Where(s => assignedScheduleIds.Contains(s.EventScheduleId))
                        .Select(s => new PublicScheduleResponse(
                            EventScheduleId: s.EventScheduleId,
                            StartDate: s.StartDate,
                            EndDate: s.EndDate,
                            Venue: s.Venue,
                            City: s.City,
                            Country: s.Country,
                            IsOngoing: s.IsOngoing,
                            IsUpcoming: s.IsUpcoming,
                            IsCompleted: s.IsCompleted
                        ))
                        .ToList();

                    if (assignedSchedules.Count == 0) continue;

                    result.Add(new PublicEventResponse(
                        EventId: evt.EventId,
                        EventName: evt.EventName,
                        EventLogoLightUrl: evt.EventLogoLightUrl,
                        EventLogoDarkUrl: evt.EventLogoDarkUrl,
                        Schedules: assignedSchedules
                    ));
                }

                return Result<IReadOnlyList<PublicEventResponse>>.Success(result);
            }
            catch
            {
                return ScheduleErrors.ExternalApiFailed;
            }
        }
    }
}