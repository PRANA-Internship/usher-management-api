using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
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

        public async Task AddAsync(ScheduleAssignment assignment, CancellationToken ct = default) =>
            await db.ScheduleAssignments.AddAsync(assignment, ct);

        public Task UpdateAsync(ScheduleAssignment assignment, CancellationToken ct = default)
        {
            db.ScheduleAssignments.Update(assignment);
            return Task.CompletedTask;
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
