using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Contracts.Usher;
using UMS.Domain.Common;
using UMS.Domain.Enums;

namespace UMS.Application.Features.Auth.Commands.SubmitApplication
{

    public sealed class SubmitUsherApplicationCommand : IRequest<Result<SubmitUsherApplicationResponse>>
    {

        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public Gender Gender { get; init; }
        public DateOnly DateOfBirth { get; init; }
        public string Address { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string EmergencyContactName { get; init; } = string.Empty;
        public string EmergencyContactPhone { get; init; } = string.Empty;
        public string EducationLevel { get; init; } = string.Empty;
        public string ExperienceSummary { get; init; } = string.Empty;
        public string Languages { get; init; } = string.Empty;
        public string Sector { get; init; } = string.Empty;
        public IFormFile ProfilePhoto { get; init; } = null!;
        public IFormFile IdDocument { get; init; } = null!;
    }
    public sealed class SubmitUsherApplicationValidator
    : AbstractValidator<SubmitUsherApplicationCommand>
    {
        private static readonly string[] AllowedImageTypes =
            ["image/jpeg", "image/png", "image/webp"];

        private static readonly string[] AllowedDocumentTypes =
            ["image/jpeg", "image/png", "application/pdf"];

        private const long MaxImageSize = 5 * 1024 * 1024;  // 5 MB 
        private const long MaxDocumentSize = 10 * 1024 * 1024; // 10 MB

        public SubmitUsherApplicationValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Invalid email address.");
            RuleFor(x => x.Phone).NotEmpty().MaximumLength(30).WithMessage("Phone number is required and must not exceed 30 characters.");
            RuleFor(x => x.Address).NotEmpty().MaximumLength(255);
            RuleFor(x => x.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.EmergencyContactName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.EmergencyContactPhone).NotEmpty().MaximumLength(30);
            RuleFor(x => x.EducationLevel).NotEmpty().MaximumLength(100);
            RuleFor(x => x.ExperienceSummary).NotEmpty().MaximumLength(300);
            RuleFor(x => x.Languages).NotEmpty().MaximumLength(255);
            RuleFor(x => x.Sector).NotEmpty().MaximumLength(100);
            RuleFor(x => x.DateOfBirth)
                .Must(dob => dob < DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-13)));

            RuleFor(x => x.ProfilePhoto)
                .NotNull().WithMessage("Profile photo is required.")
                .Must(f => f.Length <= MaxImageSize)
                    .WithMessage("Profile photo must not exceed 5MB.")
                .Must(f => AllowedImageTypes.Contains(f.ContentType.ToLowerInvariant()))
                    .WithMessage("Profile photo must be JPEG, PNG, or WebP.");

            RuleFor(x => x.IdDocument)
                .NotNull().WithMessage("ID document is required.")
                .Must(f => f.Length <= MaxDocumentSize)
                    .WithMessage("ID document must not exceed 10MB.")
                .Must(f => AllowedDocumentTypes.Contains(f.ContentType.ToLowerInvariant()))
                    .WithMessage("ID document must be JPEG, PNG, or PDF.");
        }
    }
}
