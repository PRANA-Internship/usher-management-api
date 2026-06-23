using MediatR;
using UMS.Contracts.Coordinator.Dashboard;
using UMS.Domain.Common;

namespace UMS.Application.Features.Coordinator.Queries.DashboardAnalytics;

public sealed record GetCoordinatorDashboardAnalyticsQuery : IRequest<Result<CoordinatorDashboardResponse>>;
