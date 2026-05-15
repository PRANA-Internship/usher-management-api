using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Enums;

namespace UMS.Contracts.Usher
{
    public sealed record GetUsherApplicationsResponse(
        IReadOnlyList<UsherApplicationSummary> Items,
        int TotalCount,
        int Page,
        int Size,
        int TotalPages
    );
    public sealed record GetUsherApplicationDetailResponse(
        Guid UsherId,
        Guid UserId,
        string FullName,
        string Email,
        string Phone,
        string Gender,
        DateOnly DateOfBirth,
        string Address,
        string City,
        string EmergencyContactName,
        string EmergencyContactPhone,
        string EducationLevel,
        string ExperienceSummary,
        string Languages,
        string Sector,
        string ProfilePhotoPath,
        string IdDocumentPath,
        ApprovalStatus ApplicationStatus,
        DateTimeOffset SubmittedAt
    );
}
