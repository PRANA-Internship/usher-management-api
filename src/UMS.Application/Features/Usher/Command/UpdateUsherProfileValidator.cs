using System;
using System.Collections.Generic;
using System.Text;

using FluentValidation;

using MediatR;

using Microsoft.AspNetCore.Http;

using UMS.Domain.Common;
using UMS.Domain.Enums;

namespace UMS.Application.Features.Ushers.Command
{

    public sealed class UpdateUsherProfileCommand : IRequest<Result<bool>>
    {
        public Guid UserId { get; init; }
        public string? Phone { get; init; }
        public string? Address { get; init; }
        public string? City { get; init; }
        public string? EmergencyContactName { get; init; }
        public string? EmergencyContactPhone { get; init; }
        public EducationLevel? EducationLevel { get; init; }
        public string? ExperienceSummary { get; init; }
        public List<Language>? Languages { get; init; }
        public List<Sector>? Sector { get; init; }
        public IFormFile? ProfilePhoto { get; init; }
        public string? FullName { get; init; }
        public string? CurrentPassword { get; init; }
        public string? NewPassword { get; init; }
        public Gender? Gender { get; init; }
        public DateOnly? DateOfBirth { get; init; }
        public IFormFile? IdDocument { get; init; }
    }
    public sealed class UpdateUsherProfileValidator
    : AbstractValidator<UpdateUsherProfileCommand>
    {
        private static readonly string[] AllowedImageTypes =
            ["image/jpeg", "image/png", "image/webp"];

        private const long MaxImageSize = 5 * 1024 * 1024;

        public UpdateUsherProfileValidator()
        {
            When(x => x.Phone is not null, () =>
                RuleFor(x => x.Phone).NotEmpty().MaximumLength(30));

            When(x => x.Address is not null, () =>
                RuleFor(x => x.Address).NotEmpty().MaximumLength(255));

            When(x => x.City is not null, () =>
                RuleFor(x => x.City).NotEmpty().MaximumLength(100));

            When(x => x.EmergencyContactName is not null, () =>
                RuleFor(x => x.EmergencyContactName).NotEmpty().MaximumLength(100));

            When(x => x.EmergencyContactPhone is not null, () =>
                RuleFor(x => x.EmergencyContactPhone).NotEmpty().MaximumLength(30));

            RuleFor(x => x.EducationLevel).IsInEnum()
            .WithMessage("Invalid education level.");

            When(x => x.ExperienceSummary is not null, () =>
                RuleFor(x => x.ExperienceSummary).NotEmpty().MaximumLength(250));
            When(x => x.Languages is not null && x.Languages.Count > 0, () =>
            {
                RuleFor(x => x.Languages)
                    .Must(l => l!.Distinct().Count() == l!.Count)
                        .WithMessage("Duplicate languages are not allowed.")
                    .Must(l => l!.All(v => Enum.IsDefined(typeof(Language), v)))
                        .WithMessage("One or more languages are invalid.");
            });

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

            When(x => x.ProfilePhoto is not null, () =>
            {
                RuleFor(x => x.ProfilePhoto!)
                    .Must(f => f.Length <= MaxImageSize)
                        .WithMessage("Profile photo must not exceed 5MB.")
                    .Must(f => AllowedImageTypes.Contains(f.ContentType.ToLowerInvariant()))
                        .WithMessage("Profile photo must be JPEG, PNG, or WebP.");
            });

            When(x => x.FullName is not null, () =>
    RuleFor(x => x.FullName).NotEmpty().MaximumLength(100));

            When(x => x.NewPassword is not null, () =>
            {
                RuleFor(x => x.CurrentPassword).NotEmpty()
                    .WithMessage("Current password is required to set a new password.");
                RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8)
                    .WithMessage("New password must be at least 8 characters.");
            });

            When(x => x.DateOfBirth.HasValue, () =>
                RuleFor(x => x.DateOfBirth)
                    .Must(d => d < DateOnly.FromDateTime(DateTime.UtcNow))
                    .WithMessage("Date of birth must be in the past."));

            RuleFor(x => x).Must(cmd =>
                cmd.Phone is not null ||
                cmd.Address is not null ||
                cmd.City is not null ||
                cmd.EmergencyContactName is not null ||
                cmd.EmergencyContactPhone is not null ||
                cmd.EducationLevel is not null ||
                cmd.ExperienceSummary is not null ||
                cmd.Languages is not null ||
                cmd.Sector is not null ||
                cmd.ProfilePhoto is not null ||
                cmd.FullName is not null ||
                cmd.NewPassword is not null ||
                cmd.Gender is not null ||
                cmd.DateOfBirth is not null ||
                cmd.IdDocument is not null)
            .WithMessage("At least one field must be provided.");
        }
    }
}