using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Entities;
using UMS.Domain.Enums;

namespace UMS.Application.Common.Interfaces
{
    public interface IUserRepository
    {

        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
        Task AddAsync(User user, CancellationToken ct = default);
        Task UpdateAsync(User user, CancellationToken ct = default);
        Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);

        Task DeleteAsync(User user, CancellationToken ct = default);
        Task<(IReadOnlyList<User> Items, int TotalCount)> GetCoordinatorsPagedAsync(
            int page,
            int size,
            string? searchName,
            CancellationToken ct = default);
        Task<(IReadOnlyList<User> Items, int TotalCount)> GetStaffPagedAsync(
            int page,
            int size,
            UserRole? role,
            UserStatus? status,
            string? searchName,
            CancellationToken ct = default);
    }
}
