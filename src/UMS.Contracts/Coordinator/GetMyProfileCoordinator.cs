using UMS.Domain.Enums;

namespace UMS.Contracts.Coordinator;

public sealed record GetMyProfileCoordinator(
    Guid Id,
    string FullName,
    string Email,
    string Phone,
    UserRole Role,
    DateTimeOffset CreatedAt
);