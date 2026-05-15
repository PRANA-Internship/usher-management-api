using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Enums;

namespace UMS.Contracts.Usher
{

    public sealed record SetPasswordRequest(
        string Token,
        string Password,
        string ConfirmPassword
    );
    public sealed record ApproveUsherApplicationRequest(Guid UsherId);
    public sealed record UsherApplicationSummary(
        Guid UsherId,
        Guid UserId,
        string FullName,
        string adress,
        string City,
        ApprovalStatus Status,
        DateTimeOffset SubmittedAt
        );
}
