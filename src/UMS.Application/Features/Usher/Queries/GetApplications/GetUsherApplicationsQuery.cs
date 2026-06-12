using System;
using System.Collections.Generic;
using System.Text;

using FluentValidation;

using MediatR;

using UMS.Contracts.Usher;
using UMS.Domain.Common;
using UMS.Domain.Enums;

namespace UMS.Application.Features.Ushers.Queries.GetApplications
{

    public sealed record GetUsherApplicationsQuery(
        int Page,
        int Size,
        ApprovalStatus? Status,
        string? SearchName
    ) : IRequest<Result<GetUsherApplicationsResponse>>;

    public sealed class GetUsherApplicationsValidator
        : AbstractValidator<GetUsherApplicationsQuery>
    {
        public GetUsherApplicationsValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page must be at least 1.");

            RuleFor(x => x.Size)
                .InclusiveBetween(1, 50)
                .WithMessage("Size must be between 1 and 50.");

            When(x => x.SearchName is not null, () =>
                RuleFor(x => x.SearchName)
                    .MinimumLength(2).WithMessage("Search term must be at least 2 characters.")
                    .MaximumLength(100).WithMessage("Search term must not exceed 100 characters."));

            When(x => x.Status is not null, () =>
                RuleFor(x => x.Status).IsInEnum().WithMessage("Invalid status value."));
        }
    }
}