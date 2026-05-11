using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Entities;
using UMS.Domain.Enums;

namespace UMS.tests.Entities
{

    internal sealed class EmailVerificationTokenBuilder
    {
        private Guid _userId = Guid.NewGuid();
        private TokenType _type = TokenType.EmailVerification;
        private TimeSpan _validFor = TimeSpan.FromHours(24);

        public EmailVerificationTokenBuilder WithUserId(Guid id) { _userId = id; return this; }
        public EmailVerificationTokenBuilder WithType(TokenType type) { _type = type; return this; }
        public EmailVerificationTokenBuilder WithValidity(TimeSpan ts) { _validFor = ts; return this; }

        public EmailVerificationToken Build() =>
            EmailVerificationToken.Create(_userId, _type, _validFor);

        public static EmailVerificationToken Default() => new EmailVerificationTokenBuilder().Build();

        public static EmailVerificationToken AlreadyExpired()
        {
            var token = new EmailVerificationTokenBuilder().Build();

            typeof(EmailVerificationToken)
                .GetProperty(nameof(EmailVerificationToken.ExpiresAt))!
                .SetValue(token, DateTimeOffset.UtcNow.AddHours(-1));

            return token;
        }
    }


    public sealed class EmailVerificationTokenTests
    {
        [Fact]
        public void Create_WithValidArguments_SetsPropertiesCorrectly()
        {
            var userId = Guid.NewGuid();
            var validFor = TimeSpan.FromHours(24);
            var before = DateTimeOffset.UtcNow;

            var token = EmailVerificationToken.Create(userId, TokenType.EmailVerification, validFor);

            Assert.Equal(userId, token.UserId);
            Assert.Equal(TokenType.EmailVerification, token.TokenType);
            Assert.False(string.IsNullOrWhiteSpace(token.Token));
            Assert.True(token.ExpiresAt >= before.Add(validFor));
        }

        [Fact]
        public void Create_WithEmptyUserId_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                EmailVerificationToken.Create(Guid.Empty, TokenType.EmailVerification, TimeSpan.FromHours(1)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-3600)]
        public void Create_WithNonPositiveValidity_Throws(int seconds)
        {
            Assert.Throws<ArgumentException>(() =>
                EmailVerificationToken.Create(Guid.NewGuid(), TokenType.EmailVerification, TimeSpan.FromSeconds(seconds)));
        }

        [Fact]
        public void Create_GeneratesUniqueTokensEachTime()
        {
            var userId = Guid.NewGuid();

            var t1 = EmailVerificationToken.Create(userId, TokenType.EmailVerification, TimeSpan.FromHours(1));
            var t2 = EmailVerificationToken.Create(userId, TokenType.EmailVerification, TimeSpan.FromHours(1));

            Assert.NotEqual(t1.Token, t2.Token);
        }

        [Fact]
        public void Create_TokenContainsNoUnsafeBase64Characters()
        {
            var token = EmailVerificationTokenBuilder.Default();

            Assert.DoesNotContain("+", token.Token);
            Assert.DoesNotContain("/", token.Token);
            Assert.DoesNotContain("=", token.Token);
        }

        // ─── IsValid / IsExpired / IsUsed ─────────────────────────────────────────

        [Fact]
        public void IsValid_WhenFreshlyCreated_IsTrue()
        {
            var token = EmailVerificationTokenBuilder.Default();

            Assert.True(token.IsValid);
            Assert.False(token.IsExpired);
            Assert.False(token.IsUsed);
        }

        [Fact]
        public void IsExpired_WhenPastExpiresAt_IsTrue()
        {
            var token = EmailVerificationTokenBuilder.AlreadyExpired();

            Assert.True(token.IsExpired);
            Assert.False(token.IsValid);
        }

        // ─── MarkAsUsed ───────────────────────────────────────────────────────────

        [Fact]
        public void MarkAsUsed_OnValidToken_SetsUsedAt()
        {
            var token = EmailVerificationTokenBuilder.Default();
            var before = DateTimeOffset.UtcNow;

            token.MarkAsUsed();

            Assert.True(token.IsUsed);
            Assert.NotNull(token.UsedAt);
            Assert.True(token.UsedAt >= before);
            Assert.False(token.IsValid);
        }

        [Fact]
        public void MarkAsUsed_WhenAlreadyUsed_Throws()
        {
            var token = EmailVerificationTokenBuilder.Default();
            token.MarkAsUsed();

            Assert.Throws<InvalidOperationException>(() => token.MarkAsUsed());
        }

        [Fact]
        public void MarkAsUsed_WhenExpired_Throws()
        {
            var token = EmailVerificationTokenBuilder.AlreadyExpired();

            Assert.Throws<InvalidOperationException>(() => token.MarkAsUsed());
        }
    }
}
