using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Enums;
using UMS.Domain.Common;

namespace UMS.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FullName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Phone { get; private set; } = string.Empty;
        //Null until user verify his email and set a password and
        //only for guest users(ushers) and other roles will have a password hash from the start
        public string? PasswordHash { get; private set; }
        public UserRole Role { get; private set; } = UserRole.GUEST;
        public UserStatus Status { get; private set; } = UserStatus.ACTIVE;
        public bool EmailVerified { get; private set; } = false;
        public DateTimeOffset? EmailVerifiedAt { get; private set; }
        public string? RefreshToken { get; set; }
        public DateTimeOffset? RefreshTokenExpiry { get; set; }

        public Usher? Usher { get; private set; }
        public ICollection<EmailVerificationToken> VerificationTokens { get; private set; } = null!;
        private User() { }


        // user who submit usher application is guest untill approved
        //doesnt require password to submit application
        public static User CreateGuest(CreateGuest data)
        {
            var fullName = data.FullName.Trim();
            var email = data.Email.Trim().ToLowerInvariant();
            var phone = data.Phone.Trim();

            ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
            ArgumentException.ThrowIfNullOrWhiteSpace(email);


            return new User
            {
                FullName = fullName,
                Email = email,
                Role = UserRole.GUEST,
                Phone = phone,
                EmailVerified = false
            };

        }



        //This method is to verify email by setting password
        //Guest can't verify his email because he need to be approved by admin
        // the set to Role->Usher
        public Error VerifyEmailAndSetPassword(string passwordHash)
        {

            ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
            if (Role == UserRole.GUEST)
            {
                return new Error("Role Guest or coordinator", "Guest can't create a password and verify email");
            }

            if (EmailVerified)
                return new Error("Email verified.", "email is alredy verified");

            PasswordHash = passwordHash;
            EmailVerified = true;
            EmailVerifiedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;

            return Error.None;

        }

    }
}
public record CreateGuest(
   string FullName,
    string Email,
        string Phone
    );
