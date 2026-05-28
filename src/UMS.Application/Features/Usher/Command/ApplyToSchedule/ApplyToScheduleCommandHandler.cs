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

namespace UMS.Application.Features.Ushers.Command.ApplyToSchedule
{

    public sealed class ApplyToScheduleCommandHandler(
        IUsherRepository usherRepository,
        IScheduleAssignmentRepository assignmentRepository,
        IUsherScheduleApplicationRepository applicationRepository,
        IUsherAvailablityService availabilityService,
        IEventsApiClient eventsApiClient,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<ApplyToScheduleCommand, Result<ApplyToScheduleResponse>>
    {
        public async Task<Result<ApplyToScheduleResponse>> Handle(
            ApplyToScheduleCommand command,
            CancellationToken cancellationToken)
        {
            var usher = await usherRepository.GetByUserIdAsync(command.UsherId, cancellationToken);

            if (usher is null)
                return UsherScheduleErrors.UsherNotApproved;

            if (usher.ApprovalStatus != ApprovalStatus.APPROVED)
                return UsherScheduleErrors.UsherNotApproved;

            var assignment = await assignmentRepository.GetByScheduleIdAsync(
                command.ExternalScheduleId, cancellationToken);

            if (assignment is null)
                return UsherScheduleErrors.ScheduleNotFound;

            var alreadyApplied = await applicationRepository.ExistsAsync(
                command.ExternalScheduleId, usher.Id, cancellationToken);

            if (alreadyApplied)
                return UsherScheduleErrors.AlreadyApplied;

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
                return UsherScheduleErrors.ExternalApiFailed;
            }

            if (schedule is null)
                return UsherScheduleErrors.ScheduleNotFound;

            var startDate = DateOnly.Parse(schedule.StartDate);
            var endDate = DateOnly.Parse(schedule.EndDate);

            var isAvailable = await availabilityService.IsAvailableAsync(
                usher.Id, startDate, endDate, cancellationToken);

            if (!isAvailable)
                return UsherScheduleErrors.NotAvailable;

            var application = UsherScheduleApplication.Create(
                externalScheduleId: command.ExternalScheduleId,
                externalEventId: command.ExternalEventId,
                usherId: usher.Id,
                startDate: startDate,
                endDate: endDate);

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await applicationRepository.AddAsync(application, cancellationToken);
            }, cancellationToken);

            return Result<ApplyToScheduleResponse>.Success(new ApplyToScheduleResponse(
                ApplicationId: application.Id,
                ExternalScheduleId: application.ExternalScheduleId,
                ExternalEventId: application.ExternalEventId,
                Status: application.Status.ToString(),
                AppliedAt: application.CreatedAt
            ));
        }
    }
}
