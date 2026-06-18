using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Contracts.Coordinator;

using UMS.Domain.Common;
using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Coordinator.Queries.GetMyProfile;

public sealed class GetMyProfileQueryHandler(
    IUserRepository userRepository)
    : IRequestHandler<GetMyProfileQuery, Result<GetMyProfileCoordinator>>
{
    public async Task<Result<GetMyProfileCoordinator>> Handle(
        GetMyProfileQuery query,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(
            query.UserId,
            cancellationToken);

        if (user is null)
            return UserErrors.NotFound;

        return Result<GetMyProfileCoordinator>.Success(
            new GetMyProfileCoordinator(
                Id: user.Id,
                FullName: user.FullName,
                Email: user.Email,
                Phone: user.Phone,
                Role: user.Role,
                CreatedAt: user.CreatedAt
            ));
    }
}