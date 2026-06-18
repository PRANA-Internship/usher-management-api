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

    public sealed class UsherScheduleApplicationRepository(AppDbContext db)
        : IUsherScheduleApplicationRepository
    {
        public async Task AddAsync(
            UsherScheduleApplication application, CancellationToken ct = default) =>
            await db.UsherScheduleApplications.AddAsync(application, ct);

        public Task UpdateAsync(
            UsherScheduleApplication application, CancellationToken ct = default)
        {
            db.UsherScheduleApplications.Update(application);
            return Task.CompletedTask;
        }

        public Task<UsherScheduleApplication?> GetByIdAsync(
            Guid id, CancellationToken ct = default) =>
            db.UsherScheduleApplications
              .Include(a => a.Usher).ThenInclude(u => u.User)
              .FirstOrDefaultAsync(a => a.Id == id, ct);
        public Task<bool> ExistsAsync(
            string externalScheduleId,
            Guid usherId,
            CancellationToken ct = default) =>
            db.UsherScheduleApplications.AnyAsync(a =>
                a.ExternalScheduleId == externalScheduleId &&
                a.UsherId == usherId &&
                a.Status != InvitationStatus.DECLINED, ct);

        public Task<bool> HasDateConflictAsync(
            Guid usherId,
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken ct = default) =>
            db.UsherScheduleApplications.AnyAsync(a =>
                a.UsherId == usherId &&
                a.Status == InvitationStatus.ACCEPTED &&
                a.ScheduleStartDate <= endDate &&
                a.ScheduleEndDate >= startDate, ct);

        public async Task<IReadOnlyList<UsherScheduleApplication>> GetByUsherIdAsync(
            Guid usherId,
            InvitationStatus? status,
            CancellationToken ct = default)
        {
            var query = db.UsherScheduleApplications
                .Where(a => a.UsherId == usherId)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(ct);
        }
        public Task<int> CountApprovedAsync(Guid usherId, CancellationToken ct = default) =>
    db.UsherScheduleApplications
      .CountAsync(a => a.UsherId == usherId
                    && a.Status == InvitationStatus.ACCEPTED, ct);

        public async Task<IReadOnlyList<UsherScheduleApplication>> GetApprovedPagedAsync(
            Guid usherId, int skip, int take, CancellationToken ct = default) =>
            await db.UsherScheduleApplications
                .Where(a => a.UsherId == usherId
                         && a.Status == InvitationStatus.ACCEPTED)
                .OrderBy(a => a.ScheduleStartDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        public async Task<(IReadOnlyList<UsherScheduleApplication>, int)>
    GetBySchedulePagedAsync(
        string scheduleId, InvitationStatus status,
        int page, int size, CancellationToken ct = default)
        {
            var query = db.UsherScheduleApplications
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

        public async Task<IReadOnlyList<Guid>> GetConflictedUsherIdsAsync(
            DateOnly start, DateOnly end, CancellationToken ct = default) =>
            await db.UsherScheduleApplications
                .Where(a => a.Status == InvitationStatus.ACCEPTED
                         && a.ScheduleStartDate <= end
                         && a.ScheduleEndDate >= start)
                .Select(a => a.UsherId)
                .Distinct()
                .ToListAsync(ct);

        public async Task<IReadOnlyList<Guid>> GetUsherIdsByScheduleAsync(
            string scheduleId, CancellationToken ct = default) =>
            await db.UsherScheduleApplications
                .Where(a => a.ExternalScheduleId == scheduleId
                         && a.Status != InvitationStatus.DECLINED)
                .Select(a => a.UsherId)
                .ToListAsync(ct);

        public Task<int> CountApprovedByScheduleAsync(
            string scheduleId, CancellationToken ct = default) =>
            db.UsherScheduleApplications
              .CountAsync(a => a.ExternalScheduleId == scheduleId
                            && a.Status == InvitationStatus.ACCEPTED, ct);

        public async Task<IReadOnlyList<UsherScheduleApplication>>
            GetApprovedBySchedulePagedAsync(
                string scheduleId, int skip, int take, CancellationToken ct = default) =>
            await db.UsherScheduleApplications
                .Include(a => a.Usher).ThenInclude(u => u.User)
                .Where(a => a.ExternalScheduleId == scheduleId
                         && a.Status == InvitationStatus.ACCEPTED)
                .OrderBy(a => a.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        public async Task<IReadOnlyList<UsherScheduleApplication>>
    GetConfirmedByUsherAsync(
        Guid usherId, CancellationToken ct = default) =>
    await db.UsherScheduleApplications
        .Where(a => a.UsherId == usherId
                 && a.Status == InvitationStatus.ACCEPTED)
        .ToListAsync(ct);

    }
}