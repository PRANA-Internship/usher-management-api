using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Contracts.Staff;
using UMS.Domain.Common;

namespace UMS.Application.Features.Staff.Queries.GetStaff
{

    public sealed class GetStaffQueryHandler(
        IUserRepository userRepository
    ) : IRequestHandler<GetStaffQuery, Result<GetStaffResponse>>
    {
        public async Task<Result<GetStaffResponse>> Handle(
            GetStaffQuery query,
            CancellationToken cancellationToken)
        {
            var (items, totalCount) = await userRepository.GetStaffPagedAsync(
                page: query.Page,
                size: query.Size,
                role: query.Role,
                status: query.Status,
                searchName: query.SearchName,
                ct: cancellationToken);

            var summaries = items.Select(u => new StaffSummary(
                UserId: u.Id,
                FullName: u.FullName,
                Email: u.Email,
                Phone: u.Phone,
                Role: u.Role.ToString(),
                Status: u.Status.ToString(),
                CreatedAt: u.CreatedAt
            )).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)query.Size);

            return Result<GetStaffResponse>.Success(new GetStaffResponse(
                Items: summaries,
                TotalCount: totalCount,
                Page: query.Page,
                Size: query.Size,
                TotalPages: totalPages,
                AppliedRole: query.Role?.ToString(),
                AppliedStatus: query.Status?.ToString(),
                AppliedSearch: query.SearchName
            ));
        }
    }

}
