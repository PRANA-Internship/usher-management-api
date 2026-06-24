using System;
using System.Collections.Generic;
using System.Text;

using UMS.Domain.Entities;
using UMS.Domain.Enums;

namespace UMS.Application.Common.Interfaces
{
    public interface IUsherRepository
    {
        Task AddAsync(Usher usher, CancellationToken ct = default);


        //for admin to view ushers with there id fk from user 
        Task<Usher?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

        //for admin to view ushers with there id pk from usher 
        Task UpdateAsync(Usher usher, CancellationToken ct = default);
        Task<Usher?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<(IReadOnlyList<Usher> Items, int TotalCount)> GetPagedAsync(
         int page,
         int size,
         ApprovalStatus? status,
         string? searchName,
         CancellationToken ct = default);
        Task<(IReadOnlyList<Usher> Items, int TotalCount)> GetAvailablePagedAsync(
    ISet<Guid> excludedUsherIds, int page, int size, CancellationToken ct = default);
        Task<int> CountApprovedAsync(CancellationToken ct = default);
        Task<int> CountPendingAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Guid>> GetAllApprovedIdsAsync(CancellationToken ct = default);

    }
}