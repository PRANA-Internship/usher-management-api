using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Enums;
using UMS.Domain.Common;

namespace UMS.Domain.Entities
{
    public class Usher : BaseEntity
    {
        public Guid UserId { get; private set; }

        public Gender Gender { get; private set; }
        public DateOnly DateOfBirth { get; private set; }
        public string Address { get; private set; } = string.Empty;
        public string City { get; private set; } = string.Empty;

        public string EmergencyContactName { get; private set; } = string.Empty;
        public string EmergencyContactPhone { get; private set; } = string.Empty;
        public string EducationLevel { get; private set; } = string.Empty;
        public string ExperienceSummary { get; private set; } = string.Empty;
        public string Languages { get; private set; } = string.Empty;
        public string Sector { get; private set; } = string.Empty;
        public string ProfilePhotoUrl { get; private set; } = string.Empty;
        public string IdDocumentUrl { get; private set; } = string.Empty;

        public ApprovalStatus ApprovalStatus { get; private set; } = ApprovalStatus.PENDING;
        public Guid? ApprovedBy { get; private set; }
        public DateTimeOffset? ApprovedAt { get; private set; }

        public User User { get; private set; } = null!;
        public User? ApprovedByUser { get; private set; }

        private Usher() { }
        public static Usher CreateApplication(Guid userId, CreateUsherData data)
        {

            ArgumentNullException.ThrowIfNull(data);

            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));

            if (data.DateOfBirth >= DateOnly.FromDateTime(DateTime.UtcNow))
                throw new ArgumentException("Date of birth must be in the past.", nameof(data));

            if (string.IsNullOrWhiteSpace(data.Address))
                throw new ArgumentException("Address is required.", nameof(data));

            if (string.IsNullOrWhiteSpace(data.City))
                throw new ArgumentException("City is required.", nameof(data));

            return new Usher
            {
                UserId = userId,
                Gender = data.Gender,
                DateOfBirth = data.DateOfBirth,
                Address = data.Address,
                City = data.City,
                EmergencyContactName = data.EmergencyContactName,
                EmergencyContactPhone = data.EmergencyContactPhone,
                EducationLevel = data.EducationLevel,
                ExperienceSummary = data.ExperienceSummary,
                Languages = data.Languages,
                Sector = data.Sector
            };
        }


        // update profile is not only for updateing the profile photo
        // but also for creating the profile photo for the first time,
        // so we can use the same method for both cases the same work for CreateOrUpdateIdDocument
        public void CreateOrUpdateProfilePhoto(string url)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));
            ProfilePhotoUrl = url;
        }

        // Uploading a new ID document resets approval — must be reviewed again
        public void CreateDocument(string url)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));

            // If usser already approve he can't update his cv or document  since it's updating status 

            if (ApprovalStatus == ApprovalStatus.APPROVED)
            {
                throw new ArgumentException("approved user(usher) cant update ther document or cv");
            }

            IdDocumentUrl = url;
        }


        public Error ApproveUsher(Guid ApprovedByUserID)
        {
            if (ApprovalStatus != ApprovalStatus.PENDING)
            {
                return new Error("", "only Usher in pending can be Approved");
            }


            ApprovalStatus = ApprovalStatus.APPROVED;
            ApprovedBy = ApprovedByUserID;
            ApprovedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;

            return Error.None;

        }

    }
}
public record CreateUsherData(
      Gender Gender,
      DateOnly DateOfBirth,
      string Address,
      string City,
      string EmergencyContactName,
      string EmergencyContactPhone,
      string EducationLevel,
      string ExperienceSummary,
      string Languages,
      string Sector
  );