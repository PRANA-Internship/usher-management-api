using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
