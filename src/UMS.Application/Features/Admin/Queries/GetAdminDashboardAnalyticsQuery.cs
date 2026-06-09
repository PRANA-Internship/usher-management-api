using MediatR;
using UMS.Contracts.Admin;
using UMS.Domain.Common;

namespace UMS.Application.Features.Admin.Queries
{
    public sealed record GetAdminDashboardAnalyticsQuery : IRequest<Result<AdminDashboardAnalyticsResponse>>;
}
