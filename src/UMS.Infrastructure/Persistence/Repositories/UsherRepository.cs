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

    public sealed class UsherRepository(AppDbContext db) : IUsherRepository
    {
        public async Task AddAsync(Usher usher, CancellationToken ct = default) =>
            await db.Ushers.AddAsync(usher, ct);

        public Task<Usher?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
                   db.Ushers
                  .Include(u => u.User)
                  .FirstOrDefaultAsync(u => u.UserId == userId, ct);
        public Task<Usher?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
                db.Ushers.Include(u => u.User)
         .FirstOrDefaultAsync(u => u.Id == id, ct);
        public Task UpdateAsync(Usher usher, CancellationToken ct = default)
        {
            db.Ushers.Update(usher);
            return Task.CompletedTask;
        }

        public async Task<(IReadOnlyList<Usher> Items, int TotalCount)> GetPagedAsync(
            int page, int size
           , ApprovalStatus? status
            , string? searchName
            , CancellationToken ct = default)
        {
            var query = db.Ushers.Include(u => u.User).AsQueryable();

            if (status.HasValue)
                query = query.Where(u => u.ApprovalStatus == status.Value);

            if (!string.IsNullOrWhiteSpace(searchName))
                query = query.Where(u =>
                    EF.Functions.ILike(u.User!.FullName, $"%{searchName.Trim()}%"));

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(ct);

            return ((IReadOnlyList<Usher>)items, totalCount);
        }
        public async Task<(IReadOnlyList<Usher>, int)> GetAvailablePagedAsync(
    ISet<Guid> excludedIds, int page, int size, CancellationToken ct = default)
        {
            var query = db.Ushers
                .Include(u => u.User)
                .Where(u => u.ApprovalStatus == ApprovalStatus.APPROVED
                         && !excludedIds.Contains(u.Id));

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderBy(u => u.User!.FullName)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(ct);

            return (items, total);
        }
        public Task<int> CountApprovedAsync(CancellationToken ct = default) =>
            db.Ushers.CountAsync(u => u.ApprovalStatus == ApprovalStatus.APPROVED, ct);

        public Task<int> CountPendingAsync(CancellationToken ct = default) =>
              db.Ushers.CountAsync(u => u.ApprovalStatus == ApprovalStatus.PENDING, ct);
    }

}