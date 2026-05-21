using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Contracts.Auth
{
    public sealed record InviteCoordinatorRequest(string Email);
    public sealed record RegisterCoordinatorRequest(
    string Token,
    string FullName,
    string Phone,
    string Password,
    string ConfirmPassword
);

    public sealed record RegisterCoordinatorResponse(
        Guid UserId,
        string FullName,
        string Email,
        string Role
        );

}
