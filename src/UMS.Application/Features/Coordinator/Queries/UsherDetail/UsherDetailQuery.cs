using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Contracts.Coordinator.Usher;
using UMS.Domain.Common;

namespace UMS.Application.Features.Coordinator.Queries.UsherDetail
{
    public sealed record UsherDetailQuery(
     Guid UsherId
 ) : IRequest<Result<UsherDetailResponse>>;

    public sealed class UsherDetailValidator
        : AbstractValidator<UsherDetailQuery>
    {
        public UsherDetailValidator()
        {
            RuleFor(x => x.UsherId).NotEmpty();
        }
    }
}
