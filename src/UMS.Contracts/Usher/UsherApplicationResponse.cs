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
        string? AppliedSearch,
        ApprovalStatus? AppliedStatus,
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
     EducationLevel EducationLevel,
     string? ExperienceSummary,
     List<Language> Languages,
     List<Sector>? Sector,
     string ProfilePhotoPath,
     string IdDocumentPath,
     ApprovalStatus ApplicationStatus,
     DateTimeOffset SubmittedAt,
     string? PendingEventId,
     string? PendingScheduleId
 );

}
