using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UMS.Domain.Enums;

namespace UMS.Domain.Entities
{
    public class EmailVerificationToken : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string Token { get; private set; } = null!;
        public TokenType TokenType { get; private set; }
        public DateTimeOffset ExpiresAt { get; private set; } = DateTimeOffset.MinValue;
        public DateTimeOffset? UsedAt { get; private set; }
        public User User { get; private set; } = null!;
        private EmailVerificationToken() { }


        // this method creadted for generating token for email 
        //it store user id which is work for user approved or created(coordinator0
        // to set their password
        public static EmailVerificationToken Create(Guid userId, TokenType tokenType, TimeSpan validFor)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));

            if (validFor <= TimeSpan.Zero)
                throw new ArgumentException("ValidFor must be a positive duration.", nameof(validFor));

            return new EmailVerificationToken
            {
                UserId = userId,
                TokenType = tokenType,
                Token = GenerateSecureToken(),
                ExpiresAt = DateTimeOffset.UtcNow.Add(validFor)
            };
        }

        //if the email token is used / expired or not
        public void MarkAsUsed()
        {
            if (IsUsed)
                throw new InvalidOperationException("Token has already been used.");

            if (IsExpired)
                throw new InvalidOperationException("Token has expired.");

            UsedAt = DateTimeOffset.UtcNow;
        }

        public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;
        public bool IsUsed => UsedAt.HasValue;
        public bool IsValid => !IsExpired && !IsUsed;
        private static string GenerateSecureToken()
               => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
                   .Replace("+", "-")
                   .Replace("/", "_")
                   .Replace("=", "");
    }

}
