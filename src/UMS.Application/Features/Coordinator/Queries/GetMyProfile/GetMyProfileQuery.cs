using MediatR;
using UMS.Contracts.Coordinator;
using UMS.Domain.Common;

namespace UMS.Application.Features.Coordinator.Queries.GetMyProfile;

public sealed record GetMyProfileQuery(Guid UserId)
    : IRequest<Result<GetMyProfileCoordinator>>;