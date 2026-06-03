using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Coordinator;
using UMS.Domain.Common;
using UMS.Domain.Entities;
using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Coordinator.Queries.GetConfirmed
{
    public sealed class ConfirmedRosterQueryHandler(
    IUsherScheduleApplicationRepository applicationRepository,
    IUsherInvitationRepository invitationRepository,
    IScheduleAssignmentRepository assignmentRepository
) : IRequestHandler<ConfirmedRosterQuery, Result<PagedConfirmedRosterResponse>>
    {
        public async Task<Result<PagedConfirmedRosterResponse>> Handle(
            ConfirmedRosterQuery query,
            CancellationToken cancellationToken)
        {
            var assignment = await assignmentRepository
                .GetByScheduleIdAsync(query.ExternalScheduleId, cancellationToken);

            if (assignment is null)
                return ScheduleErrors.ScheduleNotFound;

            if (assignment.CoordinatorId != query.CoordinatorId)
                return InvitationErrors.NotYourSchedule;

            var appCount = await applicationRepository
                .CountApprovedByScheduleAsync(
                    query.ExternalScheduleId, cancellationToken);

            var inviteCount = await invitationRepository
                .CountAcceptedByScheduleAsync(
                    query.ExternalScheduleId, cancellationToken);

            var totalCount = appCount + inviteCount;
            var totalPages = (int)Math.Ceiling(totalCount / (double)query.Size);

            if (totalCount == 0)
                return Result<PagedConfirmedRosterResponse>.Success(
                    new PagedConfirmedRosterResponse([], 0, query.Page, query.Size, 0));

            var skip = (query.Page - 1) * query.Size;

            var appSkip = Math.Min(skip, appCount);
            var appTake = Math.Max(0, Math.Min(query.Size, appCount - appSkip));
            var inviteSkip = Math.Max(0, skip - appCount);
            var inviteTake = query.Size - appTake;

            IReadOnlyList<UsherScheduleApplication> apps = [];
            IReadOnlyList<UsherInvitation> invites = [];

            if (appTake > 0)
                apps = await applicationRepository
                    .GetApprovedBySchedulePagedAsync(
                        query.ExternalScheduleId,
                        appSkip, appTake, cancellationToken);

            if (inviteTake > 0)
                invites = await invitationRepository
                    .GetAcceptedBySchedulePagedAsync(
                        query.ExternalScheduleId,
                        inviteSkip, inviteTake, cancellationToken);

            var items = new List<ConfirmedRosterItem>();

            foreach (var app in apps)
                items.Add(new ConfirmedRosterItem(
                    UsherId: app.Usher.UserId,
                    FullName: app.Usher.User!.FullName,
                    Email: app.Usher.User.Email,
                    Phone: app.Usher.User.Phone,
                    City: app.Usher.City,
                    Type: "APPLICATION",
                    ConfirmedAt: app.ReviewedAt ?? app.CreatedAt
                ));

            foreach (var invite in invites)
                items.Add(new ConfirmedRosterItem(
                    UsherId: invite.Usher.UserId,
                    FullName: invite.Usher.User!.FullName,
                    Email: invite.Usher.User.Email,
                    Phone: invite.Usher.User.Phone,
                    City: invite.Usher.City,
                    Type: "INVITATION",
                    ConfirmedAt: invite.UpdatedAt
                ));

            var sorted = items.OrderBy(x => x.ConfirmedAt).ToList();

            return Result<PagedConfirmedRosterResponse>.Success(
                new PagedConfirmedRosterResponse(
                    Items: sorted,
                    TotalCount: totalCount,
                    Page: query.Page,
                    Size: query.Size,
                    TotalPages: totalPages
                ));
        }
    }
}
