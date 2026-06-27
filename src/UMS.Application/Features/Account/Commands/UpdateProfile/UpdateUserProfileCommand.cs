using MediatR;

using UMS.Domain.Common;

namespace UMS.Application.Features.Account.Commands.UpdateProfile;

public sealed record UpdateUserProfileCommand(
    Guid UserId,
    string? Phone,
    string? FullName
) : IRequest<Result<bool>>;