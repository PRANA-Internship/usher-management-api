using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Entities;
using UMS.Domain.Enums;
using UMS.Domain.Common;
namespace UMS.tests.Entities
{

    internal sealed class UsherBuilder
    {
        private Guid _userId = Guid.NewGuid();
        private Gender _gender = Gender.FEMALE;
        private DateOnly _dob = new(1995, 6, 15);
        private string _address = "123 Main St";
        private string _city = "addis ababa";

        public UsherBuilder WithUserId(Guid id) { _userId = id; return this; }
        public UsherBuilder WithDateOfBirth(DateOnly dob) { _dob = dob; return this; }
        public UsherBuilder WithAddress(string address) { _address = address; return this; }
        public UsherBuilder WithCity(string city) { _city = city; return this; }

        public Usher Build() => Usher.CreateApplication(_userId, new CreateUsherData(
            _gender, _dob, _address, _city,
            "Emergency Person", "0612345678",
            "Bachelor", "3 years events", "Dutch, English", "Events", "profile-photos/6d698e58-313a-4858-ab13-6a0771a25f86.jpg", "id-documents/5179e42a-60ea-471c-84ba-03db8d185a19.pdf"
        ));

        public static Usher Default() => new UsherBuilder().Build();
    }
    public sealed class UsherTests
    {

        [Fact]
        public void CreateApplication_WithValidData_SetsPropertiesCorrectly()
        {
            var userId = Guid.NewGuid();
            var usher = new UsherBuilder().WithUserId(userId).Build();

            Assert.Equal(userId, usher.UserId);
            Assert.Equal(ApprovalStatus.PENDING, usher.ApprovalStatus);
            Assert.Null(usher.ApprovedBy);
            Assert.Null(usher.ApprovedAt);
        }

        [Fact]
        public void CreateApplication_WithEmptyUserId_Throws()
        {
            var data = ValidData();

            Assert.Throws<ArgumentException>(() =>
                Usher.CreateApplication(Guid.Empty, data));
        }

        [Fact]
        public void CreateApplication_WithFutureDateOfBirth_Throws()
        {
            var data = ValidData() with { DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) };

            Assert.Throws<ArgumentException>(() =>
                Usher.CreateApplication(Guid.NewGuid(), data));
        }

        [Fact]
        public void CreateApplication_WithTodayAsDateOfBirth_Throws()
        {
            var data = ValidData() with { DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow) };

            Assert.Throws<ArgumentException>(() =>
                Usher.CreateApplication(Guid.NewGuid(), data));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void CreateApplication_WithBlankAddress_Throws(string address)
        {
            var data = ValidData() with { Address = address };

            Assert.Throws<ArgumentException>(() =>
                Usher.CreateApplication(Guid.NewGuid(), data));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void CreateApplication_WithBlankCity_Throws(string city)
        {
            var data = ValidData() with { City = city };

            Assert.Throws<ArgumentException>(() =>
                Usher.CreateApplication(Guid.NewGuid(), data));
        }

        [Fact]
        public void CreateApplication_WithNullData_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                Usher.CreateApplication(Guid.NewGuid(), null!));
        }


        [Fact]
        public void CreateOrUpdateProfilePhoto_WithValidUrl_SetsUrl()
        {
            var usher = UsherBuilder.Default();

            usher.CreateOrUpdateProfilePhoto("https://cdn.example.com/photo.jpg");

            Assert.Equal("https://cdn.example.com/photo.jpg", usher.ProfilePhotoUrl);
        }

        [Fact]
        public void CreateOrUpdateProfilePhoto_CanBeCalledMultipleTimes_UpdatesEachTime()
        {
            var usher = UsherBuilder.Default();
            usher.CreateOrUpdateProfilePhoto("https://cdn.example.com/old.jpg");

            usher.CreateOrUpdateProfilePhoto("https://sdsddjidjisid/new.jpg");

            Assert.Equal("https://sdsddjidjisid/new.jpg", usher.ProfilePhotoUrl);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]

        public void CreateOrUpdateProfilePhoto_WithBlankUrl_Throws(string url)
        {
            var usher = UsherBuilder.Default();

            Assert.Throws<ArgumentException>(() =>
                usher.CreateOrUpdateProfilePhoto(url));
        }


        [Fact]
        public void CreateDocument_WhenPending_SetsDocumentUrl()
        {
            var usher = UsherBuilder.Default(); // PENDING by default

            usher.CreateDocument("https://cdn.example.com/id.pdf");

            Assert.Equal("https://cdn.example.com/id.pdf", usher.IdDocumentUrl);
        }

        [Fact]
        public void CreateDocument_WhenApproved_Throws()
        {
            var usher = UsherBuilder.Default();
            ApproveUsher(usher);

            Assert.Throws<ArgumentException>(() =>
                usher.CreateDocument("https://cdn.example.com/id.pdf"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]

        public void CreateDocument_WithBlankUrl_Throws(string url)
        {
            var usher = UsherBuilder.Default();

            Assert.Throws<ArgumentException>(() =>
                usher.CreateDocument(url));
        }


        [Fact]
        public void ApproveUsher_WhenPending_SetsApprovalFields()
        {
            var usher = UsherBuilder.Default();
            var adminId = Guid.NewGuid();
            var before = DateTimeOffset.UtcNow;

            var result = usher.ApproveUsher(adminId);

            Assert.Equal(Error.None, result);
            Assert.Equal(ApprovalStatus.APPROVED, usher.ApprovalStatus);
            Assert.Equal(adminId, usher.ApprovedBy);
            Assert.NotNull(usher.ApprovedAt);
            Assert.True(usher.ApprovedAt >= before);
        }

        [Fact]
        public void ApproveUsher_WhenAlreadyApproved_ReturnsError()
        {
            var usher = UsherBuilder.Default();
            ApproveUsher(usher);

            var result = usher.ApproveUsher(Guid.NewGuid());

            Assert.NotEqual(Error.None, result);
            // Approval metadata must not be overwritten
        }

        [Fact]
        public void ApproveUsher_WhenAlreadyApproved_DoesNotOverwriteApprover()
        {
            var usher = UsherBuilder.Default();
            var originalAdmin = Guid.NewGuid();
            ApproveUsher(usher, originalAdmin);

            usher.ApproveUsher(Guid.NewGuid()); // second attempt, different admin

            Assert.Equal(originalAdmin, usher.ApprovedBy);
        }


        private static CreateUsherData ValidData() => new(
            Gender.FEMALE,
            new DateOnly(1995, 6, 15),
            "123 Main St", "noah",
            "Emergency Person", "0945675878",
            "Bachelor", "3 years events", "Dutch, English", "Events", "\"profile-photos/6d698e58-313a-4858-ab13-6a0771a25f86.jpg\"", "id-documents/5179e42a-60ea-471c-84ba-03db8d185a19.pdf"
        );

        private static void ApproveUsher(Usher usher, Guid? adminId = null) =>
            usher.ApproveUsher(adminId ?? Guid.NewGuid());
    }
}
