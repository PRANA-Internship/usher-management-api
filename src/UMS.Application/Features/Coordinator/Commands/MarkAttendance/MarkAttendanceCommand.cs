using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Contracts.Coordinator.Attendance;
using UMS.Domain.Common;
using UMS.Domain.Enums;

namespace UMS.Application.Features.Coordinator.Commands.MarkAttendance
{
    public sealed record MarkAttendanceCommand(
    Guid CoordinatorId,
    string ExternalEventId,
    string ExternalScheduleId,
    DateOnly AttendanceDate,
    DayStatus DayStatus,
    Guid UsherId,
    AttendanceStatus Status
) : IRequest<Result<MarkAttendanceResponse>>;

    public sealed class MarkAttendanceValidator
        : AbstractValidator<MarkAttendanceCommand>
    {
        public MarkAttendanceValidator()
        {
            RuleFor(x => x.CoordinatorId).NotEmpty();
            RuleFor(x => x.ExternalEventId).NotEmpty();
            RuleFor(x => x.ExternalScheduleId).NotEmpty();
            RuleFor(x => x.UsherId).NotEmpty();
            RuleFor(x => x.DayStatus).IsInEnum();
            RuleFor(x => x.Status)
                .IsInEnum()
                .Must(s => s != AttendanceStatus.NotMarked)
                .WithMessage("Status must be Absent, Late, or Present.");
            RuleFor(x => x.AttendanceDate)
                .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Cannot mark attendance for a future date.");
        }
    }
}
