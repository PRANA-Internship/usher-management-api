using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using UMS.Domain.Entities;
using UMS.Domain.Enums;
using UMS.Domain.Common;
namespace UMS.tests.Entities
{
    public class UserTest
    {
        internal sealed class UserBuilder
        {
            private string _fullName = "Eyob Solomon";
            private string _email = "eyob@example.com";
            private string _phone = "0600000000";

            public UserBuilder WithFullName(string name) { _fullName = name; return this; }
            public UserBuilder WithEmail(string email) { _email = email; return this; }
            public UserBuilder WithPhone(string phone) { _phone = phone; return this; }

            public User Build() =>
                User.CreateGuest(new CreateGuest(_fullName, _email, _phone));

            // Convenience: a fully approved usher-backed user
            public static User GuestUser() => new UserBuilder().Build();
        }
        [Fact]
        public void CreateGuest_WithValidData_SetsPropertiesCorrectly()
        {
            var user = new UserBuilder()
                .WithFullName("  Jane Doe  ")
                .WithEmail("  JANE@EXAMPLE.COM  ")
                .WithPhone("0600000000")
                .Build();

            Assert.Equal("Jane Doe", user.FullName);
            Assert.Equal("jane@example.com", user.Email); // lowercased + trimmed
            Assert.Equal("0600000000", user.Phone);
            Assert.Equal(UserRole.GUEST, user.Role);
            Assert.False(user.EmailVerified);
            Assert.Null(user.PasswordHash);
        }

        [Theory]
        [InlineData("", "jane@example.com", "0600000000")]
        [InlineData("   ", "jane@example.com", "0600000000")]
        [InlineData("Jane", "", "0600000000")]
        [InlineData("Jane", "   ", "0600000000")]
        public void CreateGuest_WithMissingRequiredField_Throws(string name, string email, string phone)
        {
            Assert.Throws<ArgumentException>(() =>
                User.CreateGuest(new CreateGuest(name, email, phone)));
        }


        [Fact]
        public void VerifyEmailAndSetPassword_AsGuest_ReturnsError()
        {
            var user = UserBuilder.GuestUser(); // Role = GUEST

            var result = user.VerifyEmailAndSetPassword("hashed_password");

            Assert.NotEqual(Error.None, result);
        }

        [Fact]
        public void VerifyEmailAndSetPassword_WhenAlreadyVerified_ReturnsError()
        {
            var user = BuildApprovedUsherUser();

            user.VerifyEmailAndSetPassword("first_hash");
            var result = user.VerifyEmailAndSetPassword("second_hash");

            Assert.NotEqual(Error.None, result);
        }

        [Fact]
        public void VerifyEmailAndSetPassword_WhenEligible_SetsPasswordAndVerifiesEmail()
        {
            var user = BuildApprovedUsherUser();

            var result = user.VerifyEmailAndSetPassword("hashed_password");

            Assert.Equal(Error.None, result);
            Assert.True(user.EmailVerified);
            Assert.NotNull(user.EmailVerifiedAt);
            Assert.Equal("hashed_password", user.PasswordHash);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]

        public void VerifyEmailAndSetPassword_WithBlankHash_Throws(string hash)
        {
            var user = BuildApprovedUsherUser();

            Assert.Throws<ArgumentException>(() =>
                user.VerifyEmailAndSetPassword(hash));
        }


        private static User BuildApprovedUsherUser()
        {
            var user = UserBuilder.GuestUser();
            SetPrivateProperty(user, nameof(User.Role), UserRole.USHER);
            return user;
        }

        private static void SetPrivateProperty<T>(object obj, string propertyName, T value)
        {
            typeof(User)
                .GetProperty(propertyName)!
                .SetValue(obj, value);
        }
    }
}
