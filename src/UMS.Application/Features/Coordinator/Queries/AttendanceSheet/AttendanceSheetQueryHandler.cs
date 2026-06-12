using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Application.Common.Models;
using UMS.Contracts.Coordinator.Attendance;
using UMS.Domain.Common;
using UMS.Domain.Enums;

using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Coordinator.Queries.AttendanceSheet
{

    public sealed class AttendanceSheetQueryHandler(
        IScheduleAssignmentRepository assignmentRepository,
        IUsherAttendanceRepository attendanceRepository,
        IUsherScheduleApplicationRepository applicationRepository,
        IUsherInvitationRepository invitationRepository,
        IEventsApiClient eventsApiClient
    ) : IRequestHandler<AttendanceSheetQuery, Result<AttendanceSheetResponse>>
    {
        public async Task<Result<AttendanceSheetResponse>> Handle(
            AttendanceSheetQuery query,
            CancellationToken cancellationToken)
        {

            var assignment = await assignmentRepository
                .GetByScheduleIdAsync(query.ExternalScheduleId, cancellationToken);

            if (assignment is null)
                return ScheduleErrors.ScheduleNotFound;

            if (assignment.CoordinatorId != query.CoordinatorId)
                return AttendanceErrors.NotYourSchedule;

            ScheduleDto? schedule;
            try
            {
                schedule = await eventsApiClient.GetScheduleByIdAsync(
                    query.ExternalEventId,
                    query.ExternalScheduleId,
                    cancellationToken);
            }
            catch
            {
                return ScheduleErrors.ExternalApiFailed;
            }

            if (schedule is null)
                return ScheduleErrors.ScheduleNotFound;

            var startDate = DateOnly.Parse(schedule.StartDate);
            var endDate = DateOnly.Parse(schedule.EndDate);

            if (query.AttendanceDate < startDate || query.AttendanceDate > endDate)
                return AttendanceErrors.InvalidDate;

            var attendanceRecords = await attendanceRepository
                .GetByScheduleDateStatusAsync(
                    query.ExternalScheduleId,
                    query.AttendanceDate,
                    query.DayStatus,
                    cancellationToken);

            var confirmedFromApps = await applicationRepository
                .GetApprovedBySchedulePagedAsync(
                    query.ExternalScheduleId, 0, int.MaxValue, cancellationToken);

            var confirmedFromInvites = await invitationRepository
                .GetAcceptedBySchedulePagedAsync(
                    query.ExternalScheduleId, 0, int.MaxValue, cancellationToken);

            var attendanceMap = attendanceRecords
                .ToDictionary(a => a.UsherId);

            var usherItems = new List<AttendanceUsherItem>();

            foreach (var app in confirmedFromApps)
            {
                attendanceMap.TryGetValue(app.UsherId, out var record);

                usherItems.Add(new AttendanceUsherItem(
                    UsherId: app.Usher.UserId,
                    AttendanceId: record?.Id ?? Guid.Empty,
                    FullName: app.Usher.User!.FullName,
                    Phone: app.Usher.User.Phone,
                    City: app.Usher.City,
                    IsMarked: record?.IsMarked ?? false,
                    Status: record?.Status.ToString() ?? AttendanceStatus.NotMarked.ToString(),
                    Score: record?.Score ?? 0,
                    MarkedAt: record?.MarkedAt
                ));
            }

            foreach (var invite in confirmedFromInvites)
            {
                if (usherItems.Any(u => u.UsherId == invite.Usher.UserId))
                    continue;

                attendanceMap.TryGetValue(invite.UsherId, out var record);

                usherItems.Add(new AttendanceUsherItem(
                    UsherId: invite.Usher.UserId,
                    AttendanceId: record?.Id ?? Guid.Empty,
                    FullName: invite.Usher.User!.FullName,
                    Phone: invite.Usher.User.Phone,
                    City: invite.Usher.City,
                    IsMarked: record?.IsMarked ?? false,
                    Status: record?.Status.ToString() ?? AttendanceStatus.NotMarked.ToString(),
                    Score: record?.Score ?? 0,
                    MarkedAt: record?.MarkedAt
                ));
            }

            var sorted = usherItems.OrderBy(u => u.FullName).ToList();
            var totalConfirmed = sorted.Count;
            var totalMarked = sorted.Count(u => u.IsMarked);

            return Result<AttendanceSheetResponse>.Success(new AttendanceSheetResponse(
                ExternalScheduleId: query.ExternalScheduleId,
                ExternalEventId: query.ExternalEventId,
                AttendanceDate: query.AttendanceDate,
                DayStatus: query.DayStatus.ToString(),
                TotalConfirmed: totalConfirmed,
                TotalMarked: totalMarked,
                TotalNotMarked: totalConfirmed - totalMarked,
                Ushers: sorted
            ));
        }
    }

}