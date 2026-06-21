using MediatR;

using UMS.Domain.Common;

namespace UMS.Application.Features.Coordinator.Commands.UpdateProfile;

public sealed record UpdateCoordinatorProfileCommand(
    Guid UserId,
    string? Phone,
    string? FullName,
    string? CurrentPassword,
    string? NewPassword
) : IRequest<Result<bool>>;