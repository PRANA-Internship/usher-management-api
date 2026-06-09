using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Application.Common.Interfaces
{


    public interface INotificationService
    {
        Task NotifyAdminsNewUsherApplicationAsync(
            string usherFullName, CancellationToken ct = default);

        Task NotifyAdminsStaffPasswordSetAsync(
            string staffFullName, string role, CancellationToken ct = default);

        Task NotifyCoordinatorScheduleAssignedAsync(
            Guid coordinatorId,
            string venue,
            string startDate,
            CancellationToken ct = default);

        Task NotifyCoordinatorUsherAcceptedAsync(
            Guid coordinatorId,
            string usherFullName,
            CancellationToken ct = default);

        Task NotifyCoordinatorUsherAppliedAsync(
            Guid coordinatorId,
            string usherFullName,
            CancellationToken ct = default);

        Task NotifyUsherInvitedAsync(
            Guid userId,
            string venue,
            string startDate,
            CancellationToken ct = default);

        Task NotifyUsherApplicationApprovedAsync(
            Guid userId,
            CancellationToken ct = default);
    }
}
