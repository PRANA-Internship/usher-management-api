using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;

using UMS.Application.Common.Interfaces;
using UMS.Domain.Entities;
using UMS.Domain.Enums;
using UMS.Infrastructure.Persistance.Context;

namespace UMS.Infrastructure.Persistence.Repositories
{

    public sealed class UserRepository(AppDbContext db) : IUserRepository
    {
        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
            db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            db.Users.FindAsync([id], ct).AsTask();

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default) =>
            db.Users.AnyAsync(u => u.Email == email, ct);

        public async Task AddAsync(User user, CancellationToken ct = default)
        {
            await db.Users.AddAsync(user, ct);
            await db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(User user, CancellationToken ct = default)
        {
            db.Users.Update(user);
            await db.SaveChangesAsync(ct);
        }
        public Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default) =>
                db.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, ct);
        public Task DeleteAsync(User user, CancellationToken ct = default)
        {
            db.Users.Remove(user);
            return Task.CompletedTask;
        }
        public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetCoordinatorsPagedAsync(
    int page,
    int size,
    string? searchName,
    CancellationToken ct = default)
        {
            var query = db.Users
                .Where(u => u.Role == UserRole.EVENT_COORDINATOR)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
                query = query.Where(u =>
                    EF.Functions.ILike(u.FullName, $"%{searchName.Trim()}%"));

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(ct);

            return ((IReadOnlyList<User>)items, totalCount);
        }
        public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetStaffPagedAsync(
            int page,
            int size,
            UserRole? role,
            UserStatus? status,
            string? searchName,
            CancellationToken ct = default)
        {
            var query = db.Users
                .Where(u => u.Role == UserRole.ADMIN ||
                            u.Role == UserRole.EVENT_COORDINATOR)
                .AsQueryable();

            if (role.HasValue)
                query = query.Where(u => u.Role == role.Value);

            if (status.HasValue)
                query = query.Where(u => u.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(searchName))
                query = query.Where(u =>
                    EF.Functions.ILike(u.FullName, $"%{searchName.Trim()}%"));

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(ct);

            return ((IReadOnlyList<User>)items, totalCount);

        }

        public async Task<IReadOnlyList<Guid>>
            GetUserIdsByRoleAsync(
                UserRole role, CancellationToken ct = default) =>
            await db.Users
                .Where(u => u.Role == role)
                .Select(u => u.Id)
                .ToListAsync(ct);

    }
}