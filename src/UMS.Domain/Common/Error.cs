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
        }

    }

}
