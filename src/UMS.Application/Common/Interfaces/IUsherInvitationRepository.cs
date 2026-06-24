using System;
using System.Collections.Generic;
using System.Text;

using UMS.Domain.Entities;
using UMS.Domain.Enums;

namespace UMS.Application.Common.Interfaces
{

    public interface IUsherInvitationRepository
    {
        Task AddAsync(UsherInvitation invitation, CancellationToken ct = default);
        Task UpdateAsync(UsherInvitation invitation, CancellationToken ct = default);

        Task<UsherInvitation?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<bool> ExistsAsync(
            string externalScheduleId,
            Guid usherId,
            CancellationToken ct = default);
        Task<bool> HasDateConflictAsync(
            Guid usherId,
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken ct = default);
        Task<(IReadOnlyList<UsherInvitation> Items, int TotalCount)> GetByScheduleIdPagedAsync(
            string externalScheduleId,
            int page,
            int size,
            InvitationStatus? status,
            CancellationToken ct = default);

        Task<IReadOnlyList<UsherInvitation>> GetByScheduleIdAsync(
            string externalScheduleId,
            CancellationToken ct = default);

        Task<(int TotalInvited, int TotalAccepted, int TotalDeclined, int TotalPending)>
            GetCountsByScheduleIdAsync(string externalScheduleId, CancellationToken ct = default);
        Task<IReadOnlyList<UsherInvitation>> GetByUsherIdAsync(
            Guid usherId,
            CancellationToken ct = default);

        Task<IReadOnlyList<UsherInvitation>> GetByUsherIdAndStatusAsync(
            Guid usherId,
            InvitationStatus status,
            CancellationToken ct = default);
        Task<int> CountAcceptedAsync(Guid usherId, CancellationToken ct = default);
        Task<IReadOnlyList<UsherInvitation>> GetAcceptedPagedAsync(
            Guid usherId, int skip, int take, CancellationToken ct = default);
        Task<(IReadOnlyList<UsherInvitation> Items, int TotalCount)> GetBySchedulePagedAsync(
           string scheduleId, InvitationStatus status,
           int page, int size, CancellationToken ct = default);
        Task<int> CountAcceptedByScheduleAsync(
             string scheduleId, CancellationToken ct = default);
        Task<IReadOnlyList<Guid>> GetUsherIdsByScheduleAsync(
    string scheduleId, CancellationToken ct = default);
        Task<IReadOnlyList<Guid>> GetAcceptedUsherIdsByScheduleAsync(
            string scheduleId, CancellationToken ct = default);
        Task<IReadOnlyList<Guid>> GetConflictedUsherIdsAsync(
            DateOnly start, DateOnly end, CancellationToken ct = default);

        Task<IReadOnlyList<UsherInvitation>> GetAcceptedBySchedulePagedAsync(
            string scheduleId, int skip, int take, CancellationToken ct = default);
        Task<IReadOnlyList<UsherInvitation>> GetAcceptedByUsherAsync(
            Guid usherId, CancellationToken ct = default);

    }

}