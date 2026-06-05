using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Entities;

namespace UMS.Application.Common.Interfaces
{

    public interface IUsherPerformanceReviewRepository
    {
        Task AddAsync(UsherPerformanceReview review, CancellationToken ct = default);

        Task<UsherPerformanceReview?> GetByUsherAndScheduleAsync(
            Guid usherId,
            string scheduleId,
            CancellationToken ct = default);

        Task<bool> ExistsAsync(
            Guid usherId,
            string scheduleId,
            CancellationToken ct = default);

        Task<IReadOnlyList<UsherPerformanceReview>> GetByScheduleAsync(
            string scheduleId,
            CancellationToken ct = default);
        Task<double> GetAverageRatingAsync(CancellationToken ct = default);
    }
}
