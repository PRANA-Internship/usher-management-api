using System;
using System.Collections.Generic;
using System.Text;

using UMS.Domain.Enums;

namespace UMS.Contracts.Staff
{
    public sealed record CreateStaffRequest(
        string FullName,
        string Email,
        string Phone,
        UserRole Role
    );
    public sealed record CreateStaffResponse(
        Guid UserId,
        string FullName,
        string Email,
        string Phone,
        string Role,
        string Status
    );
    public sealed record ResendSetupLinkRequest(Guid StaffUserId);
    public sealed record RemoveStaffRequest(Guid StaffUserId);
}