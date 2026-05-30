using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Application.Common.Models;
using UMS.Contracts.Usher;
using UMS.Domain.Common;
using UMS.Domain.Enums;
using static UMS.Domain.Common.Error;

namespace UMS.Application.Features.Ushers.Queries.GetConfirmedApplication
{
    public sealed record GetMyConfirmedSchedulesQuery(
    Guid UsherId,
    int Page,
    int Size
) : IRequest<Result<PagedConfirmedScheduleResponse>>;

    public sealed class GetMyConfirmedSchedulesValidator
        : AbstractValidator<GetMyConfirmedSchedulesQuery>
    {
        public GetMyConfirmedSchedulesValidator()
        {
            RuleFor(x => x.UsherId).NotEmpty();
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.Size).InclusiveBetween(1, 50);
        }
    }
}
