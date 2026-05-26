using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Entities;

namespace UMS.Application.Common.Interfaces
{
    public interface IScheduleAssignmentRepository
    {
        Task<ScheduleAssignment?> GetByScheduleIdAsync(string externalScheduleId, CancellationToken ct = default);
        Task AddAsync(ScheduleAssignment assignment, CancellationToken ct = default);
        Task UpdateAsync(ScheduleAssignment assignment, CancellationToken ct = default);
    }

}
