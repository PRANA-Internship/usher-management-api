using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Domain.Entities;
using UMS.Domain.Enums;
using UMS.Infrastructure.Persistance.Context;

namespace UMS.Infrastructure.Persistence.Repositories
{

    public sealed class EmailVerificationTokenRepository(AppDbContext db)
        : IEmailVerificationTokenRepository
    {
        public async Task AddAsync(EmailVerificationToken token, CancellationToken ct = default) =>
            await db.EmailVerificationTokens.AddAsync(token, ct);

        public Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken ct = default) =>
            db.EmailVerificationTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token, ct);

        public Task UpdateAsync(EmailVerificationToken token, CancellationToken ct = default)
        {
            db.EmailVerificationTokens.Update(token);
            return Task.CompletedTask;
        }
        public Task<EmailVerificationToken?> GetActiveByUserIdAsync(
             Guid userId,
             TokenType tokenType,
             CancellationToken ct = default) =>
    db.EmailVerificationTokens
      .Where(t => t.UserId == userId &&
                  t.TokenType == tokenType &&
                  t.UsedAt == null)
      .OrderByDescending(t => t.ExpiresAt)
      .FirstOrDefaultAsync(ct);

        public async Task DeleteByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            var tokens = await db.EmailVerificationTokens
                .Where(t => t.UserId == userId)
                .ToListAsync(ct);

            db.EmailVerificationTokens.RemoveRange(tokens);
        }
    }
}
