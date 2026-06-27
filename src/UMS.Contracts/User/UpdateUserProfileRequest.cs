namespace UMS.Contracts.User;

public sealed record UpdateUserProfileRequest(
    string? FullName = null,
    string? Phone = null
);