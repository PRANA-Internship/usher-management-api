using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Entities;
using UMS.Domain.Enums;

namespace UMS.Application.Common.Interfaces
{
    public interface IEmailVerificationTokenRepository
    {
        Task AddAsync(EmailVerificationToken token, CancellationToken ct = default);
        Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken ct = default);
        Task UpdateAsync(EmailVerificationToken token, CancellationToken ct = default);
        Task<EmailVerificationToken?> GetActiveByUserIdAsync(
                     Guid userId,
                     TokenType tokenType,
                     CancellationToken ct = default);
        Task DeleteByUserIdAsync(Guid userId, CancellationToken ct = default);
    }
}
