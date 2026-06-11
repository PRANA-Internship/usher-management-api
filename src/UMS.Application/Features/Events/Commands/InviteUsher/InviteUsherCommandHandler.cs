using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Application.Common.Models;
using UMS.Contracts.Events;
using UMS.Domain.Common;
using UMS.Domain.Entities;
using UMS.Domain.Enums;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Events.Commands.InviteUsher
{


    public sealed class InviteUsherCommandHandler(
        IScheduleAssignmentRepository assignmentRepository,
        IUsherRepository usherRepository,
        IUsherInvitationRepository invitationRepository,
        IEventsApiClient eventsApiClient,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<InviteUsherCommand, Result<InviteUsherResponse>>
    {
        public async Task<Result<InviteUsherResponse>> Handle(
            InviteUsherCommand command,
            CancellationToken cancellationToken)
        {
            var assignment = await assignmentRepository.GetByScheduleIdAsync(
                command.ExternalScheduleId, cancellationToken);

            if (assignment is null || assignment.CoordinatorId != command.CoordinatorId)
                return InvitationErrors.NotYourSchedule;

            var usher = await usherRepository.GetByIdAsync(command.UsherId, cancellationToken);

            if (usher is null)
                return InvitationErrors.UsherNotFound;

            if (usher.ApprovalStatus != ApprovalStatus.APPROVED)
                return InvitationErrors.UsherNotApproved;

            var alreadyInvited = await invitationRepository.ExistsAsync(
                command.ExternalScheduleId, command.UsherId, cancellationToken);

            if (alreadyInvited)
                return InvitationErrors.AlreadyInvited;

            ScheduleDto? schedule;
            try
            {
                schedule = await eventsApiClient.GetScheduleByIdAsync(
                    command.ExternalEventId,
                    command.ExternalScheduleId,
                    cancellationToken);
            }
            catch
            {
                return ScheduleErrors.ExternalApiFailed;
            }

            if (schedule is null)
                return InvitationErrors.ScheduleNotFound;

            var startDate = DateOnly.Parse(schedule.StartDate);
            var endDate = DateOnly.Parse(schedule.EndDate);

            var hasConflict = await invitationRepository.HasDateConflictAsync(
                command.UsherId, startDate, endDate, cancellationToken);

            if (hasConflict)
                return InvitationErrors.UsherNotAvailable;

            var invitation = UsherInvitation.Create(
                externalScheduleId: command.ExternalScheduleId,
                externalEventId: command.ExternalEventId,
                usherId: command.UsherId,
                coordinatorId: command.CoordinatorId,
                startDate: startDate,
                endDate: endDate);

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await invitationRepository.AddAsync(invitation, cancellationToken);
            }, cancellationToken);

            return Result<InviteUsherResponse>.Success(new InviteUsherResponse(
                InvitationId: invitation.Id,
                UsherId: usher.Id,
                UsherFullName: usher.User!.FullName,
                UsherEmail: usher.User.Email,
                Status: invitation.Status.ToString(),
                InvitedAt: invitation.CreatedAt
            ));
        }
    }
}