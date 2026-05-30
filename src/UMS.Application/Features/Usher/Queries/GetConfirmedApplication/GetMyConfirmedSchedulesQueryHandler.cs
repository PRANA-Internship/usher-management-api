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

namespace UMS.Application.Features.Ushers.Queries.GetConfirmedApplication
{

    public sealed class GetMyConfirmedSchedulesQueryHandler(
        IUsherRepository usherRepository,
        IUsherScheduleApplicationRepository applicationRepository,
        IUsherInvitationRepository invitationRepository,
        IEventsApiClient eventsApiClient
    ) : IRequestHandler<GetMyConfirmedSchedulesQuery, Result<PagedConfirmedScheduleResponse>>
    {
        public async Task<Result<PagedConfirmedScheduleResponse>> Handle(
            GetMyConfirmedSchedulesQuery query,
            CancellationToken cancellationToken)
        {
            var usher = await usherRepository
                .GetByUserIdAsync(query.UsherId, cancellationToken);

            if (usher is null || usher.ApprovalStatus != ApprovalStatus.APPROVED)
                return UsherScheduleErrors.UsherNotApproved;
            var appTotal = await applicationRepository.CountApprovedAsync(usher.Id, cancellationToken);
            var inviteTotal = await invitationRepository.CountAcceptedAsync(usher.Id, cancellationToken);

            var totalCount = appTotal + inviteTotal;
            var totalPages = (int)Math.Ceiling(totalCount / (double)query.Size);

            if (totalCount == 0)
                return Result<PagedConfirmedScheduleResponse>.Success(
                    new PagedConfirmedScheduleResponse([], 0, query.Page, query.Size, 0));

            var (pagedApps, pagedInvites) = await GetPagedItemsAsync(
                usher.Id, appTotal, query.Page, query.Size, cancellationToken);

            var eventIds = pagedApps.Select(a => a.ExternalEventId)
                .Concat(pagedInvites.Select(i => i.ExternalEventId))
                .Distinct()
                .ToList();

            var eventDetails = new List<ExternalEventDetailDto?>();
            foreach (var id in eventIds)
                eventDetails.Add(await eventsApiClient.GetEventByIdAsync(id, cancellationToken));


            var scheduleMap = new Dictionary<string, (string EventName, ScheduleDto Schedule)>();
            foreach (var e in eventDetails.Where(e => e is not null))
                foreach (var s in e!.Schedules)
                    scheduleMap[s.EventScheduleId] = (e.Event.EventName, s);

            var items = new List<ConfirmedScheduleItem>();

            foreach (var app in pagedApps)
            {
                if (!scheduleMap.TryGetValue(app.ExternalScheduleId, out var info))
                    continue;

                items.Add(new ConfirmedScheduleItem(
                    ExternalScheduleId: app.ExternalScheduleId,
                    ExternalEventId: app.ExternalEventId,
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
                    ConfirmedAt: app.ReviewedAt ?? app.CreatedAt
                ));
            }

            foreach (var invite in pagedInvites)
            {
                if (!scheduleMap.TryGetValue(invite.ExternalScheduleId, out var info))
                    continue;

                items.Add(new ConfirmedScheduleItem(
                    ExternalScheduleId: invite.ExternalScheduleId,
                    ExternalEventId: invite.ExternalEventId,
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
                    ConfirmedAt: invite.UpdatedAt
                ));
            }

            var sorted = items.OrderBy(x => x.StartDate).ToList();

            return Result<PagedConfirmedScheduleResponse>.Success(
                new PagedConfirmedScheduleResponse(
                    Items: sorted,
                    TotalCount: totalCount,
                    Page: query.Page,
                    Size: query.Size,
                    TotalPages: totalPages
                ));
        }

        private async Task<(
            IReadOnlyList<UsherScheduleApplication>,
            IReadOnlyList<UsherInvitation>)>
            GetPagedItemsAsync(
                Guid usherId,
                int appTotal,
                int page,
                int size,
                CancellationToken ct)
        {
            var skip = (page - 1) * size;

            var appSkip = Math.Min(skip, appTotal);
            var appTake = Math.Max(0, Math.Min(size, appTotal - appSkip));

            var inviteSkip = Math.Max(0, skip - appTotal);
            var inviteTake = size - appTake;

            var appsTask = appTake > 0
                ? applicationRepository.GetApprovedPagedAsync(
                    usherId, appSkip, appTake, ct)
                : Task.FromResult<IReadOnlyList<UsherScheduleApplication>>([]);

            var invitesTask = inviteTake > 0
                ? invitationRepository.GetAcceptedPagedAsync(
                    usherId, inviteSkip, inviteTake, ct)
                : Task.FromResult<IReadOnlyList<UsherInvitation>>([]);

            var apps = await appsTask;
            var invites = await invitesTask;

            return (apps, invites);
        }
    }
}
