using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

using UMS.Application.Common;
using UMS.Application.Common.Interfaces;
using UMS.Application.Common.Models;
using UMS.Contracts.Coordinator.Attendance;
using UMS.Domain.Common;
using UMS.Domain.Entities;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Coordinator.Commands.MarkAttendance
{

    public sealed class MarkAttendanceCommandHandler(
        IScheduleAssignmentRepository assignmentRepository,
        IUsherAttendanceRepository attendanceRepository,
        IUsherScheduleApplicationRepository applicationRepository,
        IUsherInvitationRepository invitationRepository,
        IUsherRepository usherRepository,
        IEventsApiClient eventsApiClient,
        IUnitOfWork unitOfWork,
        ICacheService cache
    ) : IRequestHandler<MarkAttendanceCommand, Result<MarkAttendanceResponse>>
    {
        public async Task<Result<MarkAttendanceResponse>> Handle(
            MarkAttendanceCommand command,
            CancellationToken cancellationToken)
        {

            var assignment = await assignmentRepository
                .GetByScheduleIdAsync(command.ExternalScheduleId, cancellationToken);

            if (assignment is null || assignment.CoordinatorId != command.CoordinatorId)
                return AttendanceErrors.NotYourSchedule;

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
                return ScheduleErrors.ScheduleNotFound;
            if (schedule.IsOngoing == false)
                return AttendanceErrors.ScheduleNotOngoing;

            var startDate = DateOnly.Parse(schedule.StartDate);
            var endDate = DateOnly.Parse(schedule.EndDate);

            if (command.AttendanceDate < startDate || command.AttendanceDate > endDate)
                return AttendanceErrors.InvalidDate;

            if (command.AttendanceDate > DateOnly.FromDateTime(DateTime.UtcNow))
                return AttendanceErrors.FutureDateNotAllowed;

            var usher = await usherRepository
                .GetByUserIdAsync(command.UsherId, cancellationToken);

            if (usher is null)
                return AttendanceErrors.UsherNotConfirmed;

            var isConfirmedByApp = await applicationRepository.ExistsAsync(
                command.ExternalScheduleId, usher.Id, cancellationToken);

            var isConfirmedByInvite = await invitationRepository
                .ExistsAsync(command.ExternalScheduleId, usher.Id, cancellationToken);

            if (!isConfirmedByApp && !isConfirmedByInvite)
                return AttendanceErrors.UsherNotConfirmed;

            var existing = await attendanceRepository.GetAsync(
                command.ExternalScheduleId,
                usher.Id,
                command.AttendanceDate,
                command.DayStatus,
                cancellationToken);


            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (existing is null)
                {

                    var record = UsherAttendance.Create(
                        externalScheduleId: command.ExternalScheduleId,
                        externalEventId: command.ExternalEventId,
                        usherId: usher.Id,
                        coordinatorId: command.CoordinatorId,
                        attendanceDate: command.AttendanceDate,
                        dayStatus: command.DayStatus);

                    var markResult = record.Mark(command.Status, command.CoordinatorId);
                    if (markResult != Error.None)
                        throw new InvalidOperationException(markResult.Description);

                    await attendanceRepository.AddAsync(record, cancellationToken);
                    existing = record;
                }
                else
                {

                    var updateResult = existing.Update(command.Status, command.CoordinatorId);
                    if (updateResult != Error.None)
                        throw new InvalidOperationException(updateResult.Description);

                    await attendanceRepository.UpdateAsync(existing, cancellationToken);
                }
            }, cancellationToken);

            await cache.RemoveAsync(CacheKeys.AdminAttendanceTrend, cancellationToken);
            await cache.RemoveAsync(CacheKeys.UsherAnalytics(usher.Id), cancellationToken);
            return Result<MarkAttendanceResponse>.Success(new MarkAttendanceResponse(
                AttendanceId: existing!.Id,
                UsherId: command.UsherId,
                FullName: usher.User!.FullName,
                AttendanceDate: existing.AttendanceDate,
                DayStatus: existing.DayStatus.ToString(),
                Status: existing.Status.ToString(),
                Score: existing.Score,
                IsMarked: existing.IsMarked,
                MarkedAt: existing.MarkedAt!.Value
            ));
        }
    }

}