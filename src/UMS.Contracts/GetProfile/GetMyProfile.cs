using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Enums;

namespace UMS.Contracts.GetProfile
{
    public sealed record GetMyProfileUsher(
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
          ApprovalStatus ApplicationStatus
      );
}
