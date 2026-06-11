using System;
using System.Collections.Generic;
using System.Text;

using FluentValidation;

using MediatR;

using UMS.Contracts.Admin;
using UMS.Domain.Common;

namespace UMS.Application.Features.Admin.Queries
{

    public sealed record GetCoordinatorsQuery(
        int Page,
        int Size,
        string? SearchName
    ) : IRequest<Result<GetCoordinatorsResponse>>;
    public sealed class GetCoordinatorsValidator
    : AbstractValidator<GetCoordinatorsQuery>
    {
        public GetCoordinatorsValidator()
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
        }
    }
}