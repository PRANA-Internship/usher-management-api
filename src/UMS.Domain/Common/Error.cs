using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Domain.Common
{
    public sealed record Error(string Code, string? Description = null)
    {
        public static readonly Error None = new(string.Empty);
        public static class AuthErrors
        {
            public static readonly Error InvalidCredentials = new("AUTH_001", "Invalid email or password.");

            public static readonly Error UserInactive = new("AUTH_002", "This account is inactive.");
            public static readonly Error EmailNotVerified = new("AUTH_003", "Email address has not been verified.");
            public static readonly Error InvalidRefreshToken = new("AUTH_004", "Refresh token is invalid.");
            public static readonly Error RefreshTokenExpired = new("AUTH_005", "Refresh token has expired.");
            public static readonly Error EmailAlreadyExists = new("AUTH_006", "An account with this email already exists.");
            public static readonly Error UserNotFound = new("AUTH_007", "No account found with this email.");
            public static readonly Error InvalidResetToken = new("AUTH_008", "Password reset token is invalid or expired.");
            public static readonly Error TokenAlreadyUsed = new("AUTH_009", "Password reset token has already been used.");
            public static readonly Error CoordinatorAlreadyExists = new("AUTH_010", "An account with this email already exists.");
            public static readonly Error InvalidCoordinatorToken = new("AUTH_011", "Invitation token is invalid or expired.");
            public static readonly Error InvitationAlreadyPending = new("AUTH_013", "An active invitation has already been sent to this email. It expires in 48 hours.");


        }
        public static class UsherErrors
        {
            public static readonly Error EmailAlreadyExists = new("USHER_001", "An account with this email already exists.");
            public static readonly Error FileUploadFailed = new("USHER_002", "File upload failed. Please try again.");
            public static readonly Error ApplicationSaveFailed = new("USHER_003", "Failed to save application. Please try again.");
            public static readonly Error NotFound = new("USHER_004", "Usher application not found.");
            public static readonly Error AlreadyApproved = new("USHER_005", "Application is already approved.");
            public static readonly Error InvalidToken = new("USHER_006", "Token is invalid or expired.");
            public static readonly Error TokenAlreadyUsed = new("USHER_007", "Token has already been used.");
            public static readonly Error CannotRejectApplication = new("USHER_008", "Cannot reject an application that is not in pending status.");
        }
        public static class ScheduleErrors
        {
            public static readonly Error EventNotFound = new("SCHEDULE_001", "Event not found.");
            public static readonly Error ScheduleNotFound = new("SCHEDULE_002", "Schedule not found.");
            public static readonly Error CoordinatorNotFound = new("SCHEDULE_003", "Coordinator not found.");
            public static readonly Error AlreadyAssigned = new("SCHEDULE_004", "This schedule already has a coordinator assigned.");
            public static readonly Error ExternalApiFailed = new("SCHEDULE_005", "Failed to reach events service. Please try again.");
            public static readonly Error InvalidCoordinator = new("SCHEDULE_006", "User is not an event coordinator.");
            public static readonly Error AssignmentNotFound = new("SCHEDULE_008", "No coordinator assignment found for this schedule.");
        }

        public static class InvitationErrors
        {
            public static readonly Error UsherNotFound = new("INVITE_001", "Usher not found.");
            public static readonly Error ScheduleNotFound = new("INVITE_002", "Schedule not found.");
            public static readonly Error NotYourSchedule = new("INVITE_003", "You are not assigned to this schedule.");
            public static readonly Error AlreadyInvited = new("INVITE_004", "Usher has already been invited to this schedule.");
            public static readonly Error UsherNotAvailable = new("INVITE_005", "Usher is not available for this schedule — date conflict.");
            public static readonly Error InvitationNotFound = new("INVITE_006", "Invitation not found.");
            public static readonly Error NotYourInvitation = new("INVITE_007", "This invitation was not sent to you.");
            public static readonly Error UsherNotApproved = new("INVITE_008", "Only approved ushers can be invited.");
        }
        public static class UsherScheduleErrors
        {
            public static readonly Error ScheduleNotFound = new("USHER_SCH_001", "Schedule not found.");
            public static readonly Error ScheduleNotAssigned = new("USHER_SCH_002", "This schedule has no coordinator assigned yet.");
            public static readonly Error AlreadyApplied = new("USHER_SCH_003", "You have already applied to this schedule.");
            public static readonly Error NotAvailable = new("USHER_SCH_004", "You have a confirmed event on these dates.");
            public static readonly Error UsherNotApproved = new("USHER_SCH_005", "Only approved ushers can apply or accept invitations.");
            public static readonly Error ApplicationNotFound = new("USHER_SCH_006", "Application not found.");
            public static readonly Error InvitationNotFound = new("USHER_SCH_007", "Invitation not found.");
            public static readonly Error NotYourInvitation = new("USHER_SCH_008", "This invitation does not belong to you.");
            public static readonly Error AlreadyResponded = new("USHER_SCH_009", "You have already responded to this invitation.");
            public static readonly Error ExternalApiFailed = new("USHER_SCH_010", "Failed to reach events service. Please try again.");
        }
        public static class StaffErrors
        {
            public static readonly Error EmailAlreadyExists = new("STAFF_001", "An account with this email already exists.");
            public static readonly Error InvalidRole = new("STAFF_002", "Role must be ADMIN or EVENT_COORDINATOR.");
            public static readonly Error InvalidSetupToken = new("STAFF_003", "Setup token is invalid or expired.");
            public static readonly Error TokenAlreadyUsed = new("STAFF_004", "Setup token has already been used.");
            public static readonly Error StaffNotFound = new("STAFF_005", "Staff member not found.");
            public static readonly Error AccountInactive = new("STAFF_006", "Account is inactive. Please check your email to set your password.");
            public static readonly Error AlreadyActive = new("STAFF_007", "Account is already active.");
            public static readonly Error CannotRemoveSelf = new("STAFF_008", "You cannot remove your own account.");
            public static readonly Error CannotRemoveUsher = new("STAFF_009", "This endpoint is for staff only.");
            public static readonly Error NotYourStaff = new("STAFF_010", "You can only remove staff members you created.");
        }
        public static class AttendanceErrors
        {
            public static readonly Error AlreadyMarked = new("ATT_001", "Attendance already marked for this session.");
            public static readonly Error NotFound = new("ATT_002", "Attendance record not found.");
            public static readonly Error InvalidDate = new("ATT_003", "Attendance date must be within the schedule dates.");
            public static readonly Error FutureDateNotAllowed = new("ATT_004", "Cannot mark attendance for a future date.");
            public static readonly Error ScheduleNotOngoing = new("ATT_005", "Attendance can only be marked for ongoing or past schedule days.");
            public static readonly Error NotYourSchedule = new("ATT_006", "This schedule is not assigned to you.");
            public static readonly Error UsherNotConfirmed = new("ATT_007", "Usher is not confirmed for this schedule.");
        }
        public static class PerformanceReviewErrors
        {
            public static readonly Error AlreadyReviewed = new("REVIEW_001", "A performance review has already been submitted for this usher on this schedule.");
            public static readonly Error NotFound = new("REVIEW_002", "Performance review not found.");
            public static readonly Error ScheduleNotEnded = new("REVIEW_003", "Performance review can only be between the start and end date of an event.");
            public static readonly Error UsherNotConfirmed = new("REVIEW_004", "Usher is not confirmed for this schedule.");
            public static readonly Error NotYourSchedule = new("REVIEW_005", "This schedule is not assigned to you.");
            public static readonly Error InvalidRating = new("REVIEW_006", "All ratings must be between 1 and 5.");
        }

    }

}
