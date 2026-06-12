using System;
using System.Collections.Generic;
using System.Text;

using FluentValidation;

using MediatR;

using Microsoft.AspNetCore.Http;

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
        public EducationLevel EducationLevel { get; init; }
        public string? ExperienceSummary { get; init; }
        public List<Language> Languages { get; init; } = [];
        public List<Sector>? Sector { get; init; } = [];
        public IFormFile ProfilePhoto { get; init; } = null!;
        public IFormFile IdDocument { get; init; } = null!;
        public string? ExternalEventId { get; init; }
        public string? ExternalScheduleId { get; init; }
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
            When(x => x.ExperienceSummary is not null, () =>
                  RuleFor(x => x.ExperienceSummary).MaximumLength(2000));
            RuleFor(x => x.Languages)
                    .NotEmpty()
                    .WithMessage("At least one language is required.")
                    .Must(l => l.Distinct().Count() == l.Count)
                    .WithMessage("Duplicate languages are not allowed.")
         .Must(l => l.All(v => Enum.IsDefined(typeof(Language), v)))
                    .WithMessage("One or more languages are invalid.");

            When(x => x.Sector is not null && x.Sector.Count > 0, () =>
            {
                RuleFor(x => x.Sector)
                    .Must(s => s!.Count <= 3)
                        .WithMessage("You can select a maximum of 3 sectors.")
                    .Must(s => s!.Distinct().Count() == s!.Count)
                        .WithMessage("Duplicate sectors are not allowed.")
                    .Must(s => s!.All(v => Enum.IsDefined(typeof(Sector), v)))
                        .WithMessage("One or more sectors are invalid.");
            });


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

            RuleFor(x => x.ExternalEventId)
.MaximumLength(100);

            RuleFor(x => x.ExternalScheduleId)
                .MaximumLength(100);
        }
    }
}