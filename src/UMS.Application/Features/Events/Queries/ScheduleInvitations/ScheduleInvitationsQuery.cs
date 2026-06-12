using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Contracts.Events;
using UMS.Domain.Common;
using UMS.Domain.Enums;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Events.Queries.GetScheduleInvitations
{


    public sealed class GetScheduleInvitationsQueryHandler(
    IScheduleAssignmentRepository assignmentRepository,
    IUsherInvitationRepository invitationRepository
) : IRequestHandler<GetScheduleInvitationsQuery, Result<GetScheduleInvitationsResponse>>
    {
        public async Task<Result<GetScheduleInvitationsResponse>> Handle(
            GetScheduleInvitationsQuery query,
            CancellationToken cancellationToken)
        {
            var assignment = await assignmentRepository.GetByScheduleIdAsync(
            query.ExternalScheduleId, cancellationToken);

            if (assignment is null || assignment.CoordinatorId != query.CoordinatorId)
                return InvitationErrors.NotYourSchedule;


            var (items, totalCount) = await invitationRepository.GetByScheduleIdPagedAsync(
                externalScheduleId: query.ExternalScheduleId,
                page: query.Page,
                size: query.Size,
                status: query.Status,
                ct: cancellationToken);

            var (totalInvited, totalAccepted, totalDeclined, totalPending) =
                await invitationRepository.GetCountsByScheduleIdAsync(
                    query.ExternalScheduleId, cancellationToken);

            var summaries = items.Select(i => new UsherInvitationSummary(
                InvitationId: i.Id,
                UsherId: i.UsherId,
                UsherFullName: i.Usher.User!.FullName,
                UsherEmail: i.Usher.User.Email,
                UsherPhone: i.Usher.User.Phone,
                Status: i.Status.ToString(),
                InvitedAt: i.CreatedAt,
                RespondedAt: i.RespondedAt
            )).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)query.Size);

            return Result<GetScheduleInvitationsResponse>.Success(
                new GetScheduleInvitationsResponse(
                    ExternalScheduleId: query.ExternalScheduleId,
                    ExternalEventId: query.ExternalEventId,
                    Items: summaries,
                    TotalCount: totalCount,
                    Page: query.Page,
                    Size: query.Size,
                    TotalPages: totalPages,
                    TotalAccepted: totalAccepted,
                    TotalDeclined: totalDeclined,
                    TotalPending: totalPending,
                    AppliedStatus: query.Status?.ToString()
                ));
        }
    }
}