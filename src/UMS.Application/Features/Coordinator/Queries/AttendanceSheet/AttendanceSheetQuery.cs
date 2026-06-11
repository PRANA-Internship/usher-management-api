using System;
using System.Collections.Generic;
using System.Text;

using FluentValidation;

using MediatR;

using UMS.Contracts.Coordinator.Attendance;
using UMS.Domain.Common;
using UMS.Domain.Enums;

namespace UMS.Application.Features.Coordinator.Queries.AttendanceSheet
{
    public sealed record AttendanceSheetQuery(
     Guid CoordinatorId,
     string ExternalEventId,
     string ExternalScheduleId,
     DateOnly AttendanceDate,
     DayStatus DayStatus
 ) : IRequest<Result<AttendanceSheetResponse>>;

    public sealed class GetAttendanceSheetValidator
        : AbstractValidator<AttendanceSheetQuery>
    {
        public GetAttendanceSheetValidator()
        {
            RuleFor(x => x.CoordinatorId).NotEmpty();
            RuleFor(x => x.ExternalEventId).NotEmpty();
            RuleFor(x => x.ExternalScheduleId).NotEmpty();
            RuleFor(x => x.DayStatus).IsInEnum();
            RuleFor(x => x.AttendanceDate)
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Cannot get attendance sheet for a future date.");
        }
    }

}