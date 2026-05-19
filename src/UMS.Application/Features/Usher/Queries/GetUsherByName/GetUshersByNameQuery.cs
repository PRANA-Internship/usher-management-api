using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Contracts.Usher;
using UMS.Domain.Common;

namespace UMS.Application.Features.Ushers.Queries.GetUsherByName
{

    public sealed record GetUshersByNameQuery(string Name)
        : IRequest<Result<SearchUsherResponse>>;

    public sealed class GetUshersByNameValidator : AbstractValidator<GetUshersByNameQuery>
    {
        public GetUshersByNameValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Search term is required.")
                .MinimumLength(2).WithMessage("Search term must be at least 2 characters.")
                .MaximumLength(100);
        }
    }
}
