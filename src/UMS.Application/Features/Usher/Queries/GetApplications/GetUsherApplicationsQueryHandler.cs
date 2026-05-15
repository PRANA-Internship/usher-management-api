using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Usher;
using UMS.Domain.Common;
using UMS.Domain.Enums;

namespace UMS.Application.Features.Ushers.Queries.GetApplications
{

    public sealed class GetUsherApplicationsQueryHandler(
        IUsherRepository usherRepository
    ) : IRequestHandler<GetUsherApplicationsQuery, Result<GetUsherApplicationsResponse>>
    {
        public async Task<Result<GetUsherApplicationsResponse>> Handle(
            GetUsherApplicationsQuery query,
            CancellationToken cancellationToken)
        {
            var (items, totalCount) = await usherRepository.GetPagedAsync(
                query.Page, query.Size, query.Status, cancellationToken);

            var summaries = items.Select(u => new UsherApplicationSummary(
                UsherId: u.Id,
                UserId: u.UserId,
                FullName: u.User!.FullName,
                City: u.City,
                adress: u.Address,
                Status: u.ApprovalStatus,
                SubmittedAt: u.CreatedAt
            )).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)query.Size);

            return Result<GetUsherApplicationsResponse>.Success(new GetUsherApplicationsResponse(
                Items: summaries,
                TotalCount: totalCount,
                Page: query.Page,
                Size: query.Size,
                TotalPages: totalPages
            ));
        }
    }

}
