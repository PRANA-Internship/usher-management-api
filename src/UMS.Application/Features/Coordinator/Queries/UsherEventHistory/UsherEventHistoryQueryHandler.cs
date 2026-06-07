using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Coordinator.Usher;
using UMS.Domain.Common;
using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Coordinator.Queries.UsherEventHistory
{
    public sealed class GetUsherEventHistoryQueryHandler(
      IUsherRepository usherRepository,
      IUsherScheduleApplicationRepository applicationRepository,
      IUsherInvitationRepository invitationRepository,
      IEventsApiClient eventsApiClient
  ) : IRequestHandler<UsherEventHistoryQuery, Result<UsherEventHistoryResponse>>
    {
        public async Task<Result<UsherEventHistoryResponse>> Handle(
            UsherEventHistoryQuery query,
            CancellationToken cancellationToken)
        {
            var usher = await usherRepository
                .GetByIdAsync(query.UsherId, cancellationToken);

            if (usher is null)
                return UsherErrors.NotFound;

            var confirmedApps = await applicationRepository
                .GetConfirmedByUsherAsync(usher.Id, cancellationToken);

            var acceptedInvites = await invitationRepository
                .GetAcceptedByUsherAsync(usher.Id, cancellationToken);

            var confirmedMap = new Dictionary<string, string>();

            foreach (var app in confirmedApps)
                confirmedMap[app.ExternalScheduleId] = app.ExternalEventId;

            foreach (var invite in acceptedInvites)
                confirmedMap[invite.ExternalScheduleId] = invite.ExternalEventId;

            if (!confirmedMap.Any())
                return Result<UsherEventHistoryResponse>.Success(
                    new UsherEventHistoryResponse(
                        UsherId: usher.UserId,
                        FullName: usher.User!.FullName,
                        TotalEvents: 0,
                        Events: []
                    ));

            var schedulesByEvent = confirmedMap
             .GroupBy(kv => kv.Value)
            .ToDictionary(
                g => g.Key,
                g => g.Select(kv => kv.Key).ToList());

            var uniqueEventIds = schedulesByEvent.Keys.ToList();

            var eventDetails = await Task.WhenAll(
                uniqueEventIds.Select(id =>
                    eventsApiClient.GetEventByIdAsync(id, cancellationToken)));

            var eventDetailMap = eventDetails
                .Where(e => e is not null)
                .ToDictionary(e => e!.Event.EventId);

            var eventItems = new List<UsherEventItem>();

            foreach (var (eventId, schedules) in schedulesByEvent)
            {
                if (!eventDetailMap.TryGetValue(eventId, out var eventDetail))
                    continue;
                if (eventDetail is null)
                    continue;
                var scheduleDtoMap = (eventDetail.Schedules ?? [])
                    .ToDictionary(s => s.EventScheduleId);

                var scheduleItems = schedules
                           .Where(s => scheduleDtoMap.ContainsKey(s))
                           .Select(scheduleId =>
     {
         var dto = scheduleDtoMap[scheduleId];
         return new UsherScheduleItem(
             ExternalScheduleId: dto.EventScheduleId,
             StartDate: dto.StartDate,
             EndDate: dto.EndDate,

             IsOngoing: dto.IsOngoing,
             IsUpcoming: dto.IsUpcoming,
             IsCompleted: dto.IsCompleted
         );
     })
     .OrderBy(s => s.StartDate)
     .ToList();

                if (!scheduleItems.Any()) continue;

                eventItems.Add(new UsherEventItem(
                    ExternalEventId: eventId,
                    EventName: eventDetail.Event.EventName,
                    SchedulesParticipated: scheduleItems.Count,
                    Schedules: scheduleItems
                ));
            }

            var sorted = eventItems
                .OrderByDescending(e =>
                    e.Schedules.Max(s => s.EndDate))
                .ToList();

            return Result<UsherEventHistoryResponse>.Success(
                new UsherEventHistoryResponse(
                    UsherId: usher.UserId,
                    FullName: usher.User!.FullName,
                    TotalEvents: sorted.Count,
                    Events: sorted
                ));
        }
    }
}
