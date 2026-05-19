using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Application.Features.Ushers.Queries.GetUsherByName;
using UMS.Contracts.Usher;
using UMS.Domain.Common;

namespace UMS.Application.Features.Ushers.Queries.GetUsherByName
{

    public sealed class GetUshersByNameQueryHandler(
        IUsherRepository usherRepository
    ) : IRequestHandler<GetUshersByNameQuery, Result<SearchUsherResponse>>
    {
        public async Task<Result<SearchUsherResponse>> Handle(
            GetUshersByNameQuery query,
            CancellationToken cancellationToken)
        {
            var ushers = await usherRepository.SearchByNameAsync(
                query.Name.Trim(), cancellationToken);

            var results = ushers.Select(u => new UsherApplicationSummary(
                UsherId: u.Id,
                UserId: u.UserId,
                adress: u.Address,
                City: u.City,
                FullName: u.User!.FullName,
                Status: u.ApprovalStatus,
                SubmittedAt: u.CreatedAt
            )).ToList();

            return Result<SearchUsherResponse>.Success(new SearchUsherResponse(
                Items: results,
                TotalCount: results.Count
            ));
        }
    }
}
