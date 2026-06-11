using System;
using System.Collections.Generic;
using System.Text;

using UMS.Domain.Common;
using UMS.Domain.Enums;
using UMS.Domain.Helpers;

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
        public EducationLevel EducationLevel { get; private set; } = EducationLevel.None;
        public string? ExperienceSummary { get; private set; }
        public string ProfilePhotoUrl { get; private set; } = string.Empty;
        public string IdDocumentUrl { get; private set; } = string.Empty;

        public ApprovalStatus ApprovalStatus { get; private set; } = ApprovalStatus.PENDING;
        public Guid? ApprovedBy { get; private set; }
        public DateTimeOffset? ApprovedAt { get; private set; }

        public User User { get; private set; } = null!;
        public User? ApprovedByUser { get; private set; }

        private string _sectors = string.Empty;
        private string _languages = string.Empty;

        public IReadOnlyList<Sector> Sector => EnumHelpers.ParseEnum<Sector>(_sectors);
        public IReadOnlyList<Language> Languages => EnumHelpers.ParseEnum<Language>(_languages);

        public String? PendingEventId { get; private set; }
        public String? PendingScheduleId { get; private set; }

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
                _sectors = data.Sector is not null ? EnumHelpers.SerializeEnum(data.Sector) : string.Empty,
                _languages = EnumHelpers.SerializeEnum(data.Languages),
                ProfilePhotoUrl = data.ProfilePhotoUrl,
                IdDocumentUrl = data.IdDocumentUrl,
                PendingEventId = data.PendingEventId,
                PendingScheduleId = data.PendingScheduleId
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

        public Error RejectApplication()
        {
            if (ApprovalStatus != ApprovalStatus.PENDING)
            {
                return new Error("", "only Usher in pending can be Rejected");
            }
            ApprovalStatus = ApprovalStatus.REJECTED;
            UpdatedAt = DateTimeOffset.UtcNow;
            return Error.None;
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
        private static void ValidateSectors(IList<Sector>? sectors)
        {
            if (sectors is null) return;

            if (sectors.Count > 3)
                throw new ArgumentException("A maximum of 3 sectors can be selected.");

            if (sectors.Distinct().Count() != sectors.Count)
                throw new ArgumentException("Duplicate sectors are not allowed.");
        }


        public void UpdateProfile(
    string? address = null,
    string? city = null,
    string? emergencyContactName = null,
    string? emergencyContactPhone = null,
    EducationLevel? educationLevel = null,
    string? experienceSummary = null,
    IList<Sector>? sector = null,
    IList<Language>? languages = null)
        {
            if (sector is not null) ValidateSectors(sector);

            if (address is not null) Address = address.Trim();
            if (city is not null) City = city.Trim();
            if (emergencyContactName is not null) EmergencyContactName = emergencyContactName.Trim();
            if (emergencyContactPhone is not null) EmergencyContactPhone = emergencyContactPhone.Trim();
            if (educationLevel is not null) EducationLevel = educationLevel.Value;
            if (experienceSummary is not null) ExperienceSummary = experienceSummary.Trim();
            if (sector is not null) _sectors = EnumHelpers.SerializeEnum(sector);
            if (languages is not null) _languages = EnumHelpers.SerializeEnum(languages);

            UpdatedAt = DateTimeOffset.UtcNow;
        }
        public void UpdateProfilePhoto(string url)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(url);
            ProfilePhotoUrl = url;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

    }
}

// this recored updated because the Architecture changed
// files and photos must be uploaded first thats why
public record CreateUsherData(
    Gender Gender,
    DateOnly DateOfBirth,
    string Address,
    string City,
    string EmergencyContactName,
    string EmergencyContactPhone,
    EducationLevel EducationLevel,
    string? ExperienceSummary,
    IList<Sector>? Sector,
    IList<Language> Languages,
    string ProfilePhotoUrl,
    string IdDocumentUrl,
    string? PendingEventId,
    string? PendingScheduleId
);