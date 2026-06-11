using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

using UMS.Application.Common.Interfaces;
using UMS.Contracts.Admin;
using UMS.Domain.Common;

namespace UMS.Application.Features.Admin.Queries
{

    public sealed class GetCoordinatorsQueryHandler(
        IUserRepository userRepository
    ) : IRequestHandler<GetCoordinatorsQuery, Result<GetCoordinatorsResponse>>
    {
        public async Task<Result<GetCoordinatorsResponse>> Handle(
            GetCoordinatorsQuery query,
            CancellationToken cancellationToken)
        {
            var (items, totalCount) = await userRepository.GetCoordinatorsPagedAsync(
                page: query.Page,
                size: query.Size,
                searchName: query.SearchName,
                ct: cancellationToken);

            var summaries = items.Select(u => new CoordinatorSummary(
                UserId: u.Id,
                FullName: u.FullName,
                Email: u.Email,
                Phone: u.Phone,
                CreatedAt: u.CreatedAt
            )).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)query.Size);

            return Result<GetCoordinatorsResponse>.Success(new GetCoordinatorsResponse(
                Items: summaries,
                TotalCount: totalCount,
                Page: query.Page,
                Size: query.Size,
                TotalPages: totalPages,
                AppliedSearch: query.SearchName
            ));
        }
    }
}