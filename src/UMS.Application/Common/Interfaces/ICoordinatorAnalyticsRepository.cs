using System.Threading;
using System.Threading.Tasks;

using UMS.Contracts.Coordinator.Dashboard;

namespace UMS.Application.Common.Interfaces
{
    public interface ICoordinatorAnalyticsRepository
    {
        Task<CoordinatorDashboardResponse> GetCoordinatorDashboardAnalyticsAsync(Guid coordinatorId, CancellationToken ct = default);
    }
}