using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Domain.Entities;
using UMS.Infrastructure.Persistance.Context;

namespace UMS.Infrastructure.Persistence.Repositories
{

    public sealed class UsherPerformanceReviewRepository(AppDbContext db)
        : IUsherPerformanceReviewRepository
    {
        public async Task AddAsync(
            UsherPerformanceReview review, CancellationToken ct = default) =>
            await db.UsherPerformanceReviews.AddAsync(review, ct);

        public Task<UsherPerformanceReview?> GetByUsherAndScheduleAsync(
            Guid usherId,
            string scheduleId,
            CancellationToken ct = default) =>
            db.UsherPerformanceReviews
              .Include(r => r.Usher).ThenInclude(u => u.User)
              .FirstOrDefaultAsync(r =>
                  r.UsherId == usherId &&
                  r.ExternalScheduleId == scheduleId, ct);

        public Task<bool> ExistsAsync(
            Guid usherId,
            string scheduleId,
            CancellationToken ct = default) =>
            db.UsherPerformanceReviews.AnyAsync(r =>
                r.UsherId == usherId &&
                r.ExternalScheduleId == scheduleId, ct);

        public async Task<IReadOnlyList<UsherPerformanceReview>> GetByScheduleAsync(
            string scheduleId,
            CancellationToken ct = default) =>
            await db.UsherPerformanceReviews
                .Include(r => r.Usher).ThenInclude(u => u.User)
                .Where(r => r.ExternalScheduleId == scheduleId)
                .ToListAsync(ct);
    }
}
