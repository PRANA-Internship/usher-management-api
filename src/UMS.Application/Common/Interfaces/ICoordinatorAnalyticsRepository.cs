using System;
using System.Threading;
using System.Threading.Tasks;

using UMS.Contracts.Coordinator;

namespace UMS.Application.Common.Interfaces
{
    public interface ICoordinatorAnalyticsRepository
    {
        Task<CoordinatorDashboardAnalyticsResponse> GetCoordinatorAnalyticsAsync(
            Guid coordinatorId, CancellationToken ct = default);
    }
}