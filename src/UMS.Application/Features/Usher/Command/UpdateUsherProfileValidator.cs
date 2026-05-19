using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Common;

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
        public string? EducationLevel { get; init; }
        public string? ExperienceSummary { get; init; }
        public string? Languages { get; init; }
        public string? Sector { get; init; }
        public IFormFile? ProfilePhoto { get; init; }
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

            When(x => x.EducationLevel is not null, () =>
                RuleFor(x => x.EducationLevel).NotEmpty().MaximumLength(100));

            When(x => x.ExperienceSummary is not null, () =>
                RuleFor(x => x.ExperienceSummary).NotEmpty().MaximumLength(250));

            When(x => x.Languages is not null, () =>
                RuleFor(x => x.Languages).NotEmpty().MaximumLength(255));

            When(x => x.Sector is not null, () =>
                RuleFor(x => x.Sector).NotEmpty().MaximumLength(100));

            When(x => x.ProfilePhoto is not null, () =>
            {
                RuleFor(x => x.ProfilePhoto!)
                    .Must(f => f.Length <= MaxImageSize)
                        .WithMessage("Profile photo must not exceed 5MB.")
                    .Must(f => AllowedImageTypes.Contains(f.ContentType.ToLowerInvariant()))
                        .WithMessage("Profile photo must be JPEG, PNG, or WebP.");
            });

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
                cmd.ProfilePhoto is not null)
            .WithMessage("At least one field must be provided.");
        }
    }
}
