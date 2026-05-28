using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Application.Common.Models;
using UMS.Contracts.Usher;
using UMS.Domain.Common;
using UMS.Domain.Entities;
using UMS.Domain.Enums;
using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Ushers.Queries.GetMyPendingDecline
{

    public sealed class GetMyPendingDeclinedQueryHandler(
        IUsherRepository usherRepository,
        IUsherScheduleApplicationRepository applicationRepository,
        IUsherInvitationRepository invitationRepository,
        IEventsApiClient eventsApiClient
    ) : IRequestHandler<GetMyPendingDeclinedQuery, Result<PagedScheduleResponse>>
    {
        public async Task<Result<PagedScheduleResponse>> Handle(
            GetMyPendingDeclinedQuery query,
            CancellationToken cancellationToken)
        {
            var usher = await usherRepository
                .GetByUserIdAsync(query.UserId, cancellationToken);

            if (usher is null)
                return UsherScheduleErrors.UsherNotApproved;

            List<ScheduleItem> items;

            switch (query.Filter)
            {
                case MyScheduleStatusFilter.PendingApplication:
                    {
                        var apps = await applicationRepository.GetByUsherIdAsync(
                            usher.Id, InvitationStatus.PENDING, cancellationToken);

                        items = await MapApplicationsAsync(apps, cancellationToken);
                        break;
                    }
                case MyScheduleStatusFilter.DeclinedApplication:
                    {
                        var apps = await applicationRepository.GetByUsherIdAsync(
                            usher.Id, InvitationStatus.DECLINED, cancellationToken);

                        items = await MapApplicationsAsync(apps, cancellationToken);
                        break;
                    }
                case MyScheduleStatusFilter.PendingInvitation:
                    {
                        var invites = await invitationRepository.GetByUsherIdAndStatusAsync(
                            usher.Id, InvitationStatus.PENDING, cancellationToken);

                        items = await MapInvitationsAsync(invites, cancellationToken);
                        break;
                    }
                case MyScheduleStatusFilter.DeclinedInvitation:
                    {
                        var invites = await invitationRepository.GetByUsherIdAndStatusAsync(
                            usher.Id, InvitationStatus.DECLINED, cancellationToken);

                        items = await MapInvitationsAsync(invites, cancellationToken);
                        break;
                    }
                default:
                    return Result<PagedScheduleResponse>.Success(
                        new PagedScheduleResponse([], 0, query.Page, query.Size, 0));
            }

            var totalCount = items.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)query.Size);
            var paged = items
                .Skip((query.Page - 1) * query.Size)
                .Take(query.Size)
                .ToList();

            return Result<PagedScheduleResponse>.Success(new PagedScheduleResponse(
                Items: paged,
                TotalCount: totalCount,
                Page: query.Page,
                Size: query.Size,
                TotalPages: totalPages
            ));
        }

        private async Task<List<ScheduleItem>> MapApplicationsAsync(
            IReadOnlyList<UsherScheduleApplication> apps,
            CancellationToken ct)
        {
            var eventIds = apps.Select(a => a.ExternalEventId).Distinct().ToList();
            var scheduleMap = await BuildScheduleMapAsync(eventIds, ct);

            return apps
                .Where(a => scheduleMap.ContainsKey(a.ExternalScheduleId))
                .Select(a =>
                {
                    var info = scheduleMap[a.ExternalScheduleId];
                    return new ScheduleItem(
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
                        IsCompleted: info.Schedule.IsCompleted,
                        Type: "APPLICATION",
                        Status: a.Status.ToString()
                    );
                }).ToList();
        }

        private async Task<List<ScheduleItem>> MapInvitationsAsync(
            IReadOnlyList<UsherInvitation> invites,
            CancellationToken ct)
        {
            var eventIds = invites.Select(i => i.ExternalEventId).Distinct().ToList();
            var scheduleMap = await BuildScheduleMapAsync(eventIds, ct);

            return invites
                .Where(i => scheduleMap.ContainsKey(i.ExternalScheduleId))
                .Select(i =>
                {
                    var info = scheduleMap[i.ExternalScheduleId];
                    return new ScheduleItem(
                        ExternalScheduleId: i.ExternalScheduleId,
                        ExternalEventId: i.ExternalEventId,
                        EventName: info.EventName,
                        StartDate: info.Schedule.StartDate,
                        EndDate: info.Schedule.EndDate,
                        Venue: info.Schedule.Venue,
                        City: info.Schedule.City,
                        Country: info.Schedule.Country,
                        IsOngoing: info.Schedule.IsOngoing,
                        IsUpcoming: info.Schedule.IsUpcoming,
                        IsCompleted: info.Schedule.IsCompleted,
                        Type: "INVITATION",
                        Status: i.Status.ToString()
                    );
                }).ToList();
        }
        private async Task<Dictionary<string, (string EventName, ScheduleDto Schedule)>> BuildScheduleMapAsync(
        List<string> eventIds,
        CancellationToken ct)
        {
            var eventDetails = new List<ExternalEventDetailDto>();
            foreach (var id in eventIds)
            {
                var detail = await eventsApiClient.GetEventByIdAsync(id, ct);
                if (detail is not null)
                    eventDetails.Add(detail);
            }

            var map = new Dictionary<string, (string EventName, ScheduleDto Schedule)>();
            foreach (var e in eventDetails)
                foreach (var s in e.Schedules)
                    map[s.EventScheduleId] = (e.Event.EventName, s);

            return map;
        }
    }
}