using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;

using UMS.Application.Common.Interfaces;
using UMS.Domain.Entities;
using UMS.Domain.Enums;
using UMS.Infrastructure.Persistance.Context;

namespace UMS.Infrastructure.Persistence.Repositories
{

    public sealed class UsherInvitationRepository(AppDbContext db)
        : IUsherInvitationRepository
    {
        public async Task AddAsync(UsherInvitation invitation, CancellationToken ct = default) =>
            await db.UsherInvitations.AddAsync(invitation, ct);

        public Task UpdateAsync(UsherInvitation invitation, CancellationToken ct = default)
        {
            db.UsherInvitations.Update(invitation);
            return Task.CompletedTask;
        }

        public Task<UsherInvitation?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            db.UsherInvitations
              .Include(i => i.Usher).ThenInclude(u => u.User)
              .Include(i => i.InvitedByCoordinator)
              .FirstOrDefaultAsync(i => i.Id == id, ct);

        public Task<bool> ExistsAsync(
            string externalScheduleId,
            Guid usherId,
            CancellationToken ct = default) =>
            db.UsherInvitations.AnyAsync(i =>
                i.ExternalScheduleId == externalScheduleId &&
                i.UsherId == usherId &&
                i.Status != InvitationStatus.DECLINED, ct);

        public Task<bool> HasDateConflictAsync(
            Guid usherId,
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken ct = default) =>
            db.UsherInvitations.AnyAsync(i =>
                i.UsherId == usherId &&
                i.Status == InvitationStatus.ACCEPTED &&
                i.ScheduleStartDate <= endDate &&
                i.ScheduleEndDate >= startDate, ct);
        public async Task<IReadOnlyList<UsherInvitation>> GetByScheduleIdAsync(
            string externalScheduleId, CancellationToken ct = default) => await db.UsherInvitations
                              .Include(i => i.Usher).ThenInclude(u => u.User)
                              .Where(i => i.ExternalScheduleId == externalScheduleId)
                              .OrderByDescending(i => i.CreatedAt)
                              .ToListAsync(ct);

        public async Task<(IReadOnlyList<UsherInvitation> Items, int TotalCount)>
            GetByScheduleIdPagedAsync(
                      string externalScheduleId,
                      int page,
                      int size,
                      InvitationStatus? status,
                      CancellationToken ct = default)
        {
            var query = db.UsherInvitations
                  .Include(i => i.Usher).ThenInclude(u => u.User)
                  .Where(i => i.ExternalScheduleId == externalScheduleId)
                  .AsQueryable();

            if (status.HasValue)
                query = query.Where(i => i.Status == status.Value);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(i => i.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(ct);

            return ((IReadOnlyList<UsherInvitation>)items, totalCount);

        }
        public async Task<(int TotalInvited, int TotalAccepted, int TotalDeclined, int TotalPending)>
          GetCountsByScheduleIdAsync(string externalScheduleId, CancellationToken ct = default)
        {
            var counts = await db.UsherInvitations
                .Where(i => i.ExternalScheduleId == externalScheduleId)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Accepted = g.Count(i => i.Status == InvitationStatus.ACCEPTED),
                    Declined = g.Count(i => i.Status == InvitationStatus.DECLINED),
                    Pending = g.Count(i => i.Status == InvitationStatus.PENDING)
                })
                .FirstOrDefaultAsync(ct);

            return (
                counts?.Total ?? 0,
                counts?.Accepted ?? 0,
                counts?.Declined ?? 0,
                counts?.Pending ?? 0
            );
        }

        public async Task<IReadOnlyList<UsherInvitation>> GetByUsherIdAsync(
            Guid usherId, CancellationToken ct = default) =>
            await db.UsherInvitations
                .Include(i => i.InvitedByCoordinator)
                .Where(i => i.UsherId == usherId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync(ct);
        public async Task<IReadOnlyList<UsherInvitation>> GetByUsherIdAndStatusAsync(
            Guid usherId,
            InvitationStatus status,
            CancellationToken ct = default)
        {
            return await db.UsherInvitations
                .Where(i => i.UsherId == usherId && i.Status == status)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync(ct);
        }
        public Task<int> CountAcceptedAsync(Guid usherId, CancellationToken ct = default) =>
    db.UsherInvitations
      .CountAsync(i => i.UsherId == usherId
                    && i.Status == InvitationStatus.ACCEPTED, ct);

        public async Task<IReadOnlyList<UsherInvitation>> GetAcceptedPagedAsync(
            Guid usherId, int skip, int take, CancellationToken ct = default) =>
            await db.UsherInvitations
                .Where(i => i.UsherId == usherId
                         && i.Status == InvitationStatus.ACCEPTED)
                .OrderBy(i => i.ScheduleStartDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        public Task<int> CountAcceptedByScheduleAsync(
             string scheduleId, CancellationToken ct = default) =>
                 db.UsherInvitations
                  .CountAsync(i => i.ExternalScheduleId == scheduleId
                    && i.Status == InvitationStatus.ACCEPTED, ct);

        public async Task<IReadOnlyList<UsherInvitation>> GetAcceptedBySchedulePagedAsync(
            string scheduleId, int skip, int take, CancellationToken ct = default) =>
            await db.UsherInvitations
                .Include(i => i.Usher).ThenInclude(u => u.User)
                .Where(i => i.ExternalScheduleId == scheduleId
                         && i.Status == InvitationStatus.ACCEPTED)
                .OrderBy(i => i.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        public async Task<IReadOnlyList<Guid>> GetConflictedUsherIdsAsync(
                  DateOnly start, DateOnly end, CancellationToken ct = default) =>
              await db.UsherInvitations
         .Where(a => a.Status == InvitationStatus.ACCEPTED
                 && a.ScheduleStartDate <= end
                 && a.ScheduleEndDate >= start)
        .Select(a => a.UsherId)
        .Distinct()
        .ToListAsync(ct);
        public async Task<(IReadOnlyList<UsherInvitation>, int)>
    GetBySchedulePagedAsync(
        string scheduleId, InvitationStatus status,
        int page, int size, CancellationToken ct = default)
        {
            var query = db.UsherInvitations
                .Include(a => a.Usher).ThenInclude(u => u.User)
                .Where(a => a.ExternalScheduleId == scheduleId
                         && a.Status == status);

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(ct);

            return (items, total);
        }
        public async Task<IReadOnlyList<Guid>> GetUsherIdsByScheduleAsync(
    string scheduleId, CancellationToken ct = default) =>
    await db.UsherInvitations
        .Where(a => a.ExternalScheduleId == scheduleId
                 && a.Status != InvitationStatus.DECLINED)
        .Select(a => a.UsherId)
        .ToListAsync(ct);

        public async Task<IReadOnlyList<Guid>> GetAcceptedUsherIdsByScheduleAsync(
            string scheduleId, CancellationToken ct = default) =>
            await db.UsherInvitations
                .Where(a => a.ExternalScheduleId == scheduleId
                         && a.Status == InvitationStatus.ACCEPTED)
                .Select(a => a.UsherId)
                .ToListAsync(ct);
        public async Task<IReadOnlyList<UsherInvitation>> GetAcceptedByUsherAsync(
        Guid usherId, CancellationToken ct = default) =>
           await db.UsherInvitations
                .Where(i => i.UsherId == usherId
                 && i.Status == InvitationStatus.ACCEPTED)
                .ToListAsync(ct);

    }

}