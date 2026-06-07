using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Entities;
using UMS.Domain.Enums;

namespace UMS.Application.Common.Interfaces
{
    public interface IUsherScheduleApplicationRepository
    {
        Task AddAsync(UsherScheduleApplication application, CancellationToken ct = default);
        Task UpdateAsync(UsherScheduleApplication application, CancellationToken ct = default);

        Task<UsherScheduleApplication?> GetByIdAsync(
            Guid id, CancellationToken ct = default);

        Task<bool> ExistsAsync(
            string externalScheduleId,
            Guid usherId,
            CancellationToken ct = default);

        Task<bool> HasDateConflictAsync(
            Guid usherId,
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken ct = default);

        Task<IReadOnlyList<UsherScheduleApplication>> GetByUsherIdAsync(
            Guid usherId,
            InvitationStatus? status,
            CancellationToken ct = default);
        Task<int> CountApprovedAsync(Guid usherId, CancellationToken ct = default);
        Task<IReadOnlyList<UsherScheduleApplication>> GetApprovedPagedAsync(
            Guid usherId, int skip, int take, CancellationToken ct = default);

        Task<(IReadOnlyList<UsherScheduleApplication> Items, int TotalCount)> GetBySchedulePagedAsync(
            string scheduleId, InvitationStatus status,
            int page, int size, CancellationToken ct = default);

        Task<IReadOnlyList<Guid>> GetConflictedUsherIdsAsync(
            DateOnly start, DateOnly end, CancellationToken ct = default);

        Task<IReadOnlyList<Guid>> GetUsherIdsByScheduleAsync(
            string scheduleId, CancellationToken ct = default);

        Task<int> CountApprovedByScheduleAsync(
            string scheduleId, CancellationToken ct = default);

        Task<IReadOnlyList<UsherScheduleApplication>> GetApprovedBySchedulePagedAsync(
            string scheduleId, int skip, int take, CancellationToken ct = default);

        Task<IReadOnlyList<UsherScheduleApplication>> GetConfirmedByUsherAsync(
            Guid usherId, CancellationToken ct = default);








    }
}
