using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;

using UMS.Application.Common.Interfaces;
using UMS.Domain.Entities;
using UMS.Infrastructure.Persistance.Context;

namespace UMS.Infrastructure.Persistence.Repositories
{
    public sealed class ScheduleAssignmentRepository(AppDbContext db)
     : IScheduleAssignmentRepository
    {
        public Task<ScheduleAssignment?> GetByScheduleIdAsync(
            string externalScheduleId, CancellationToken ct = default) =>
            db.ScheduleAssignments
              .Include(a => a.Coordinator)
              .FirstOrDefaultAsync(a => a.ExternalScheduleId == externalScheduleId, ct);

        public async Task AddAsync(ScheduleAssignment assignment, CancellationToken ct = default)
        {
            await db.ScheduleAssignments.AddAsync(assignment, ct);
            await db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(ScheduleAssignment assignment, CancellationToken ct = default)
        {
            db.ScheduleAssignments.Update(assignment);
            await db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(ScheduleAssignment assignment, CancellationToken ct = default)
        {
            db.ScheduleAssignments.Remove(assignment);
            await db.SaveChangesAsync(ct);
        }
        public async Task<IReadOnlyList<ScheduleAssignment>> GetByCoordinatorIdAsync(
              Guid coordinatorId, CancellationToken ct = default) =>
                 await db.ScheduleAssignments
                     .Include(a => a.Coordinator)
                     .Where(a => a.CoordinatorId == coordinatorId)
                     .OrderByDescending(a => a.AssignedAt)
                     .ToListAsync(ct);
        public async Task<IReadOnlyList<ScheduleAssignment>> GetAllAsync(
           CancellationToken ct = default)
        {
            return await db.ScheduleAssignments
                .ToListAsync(ct);
        }
    }


}