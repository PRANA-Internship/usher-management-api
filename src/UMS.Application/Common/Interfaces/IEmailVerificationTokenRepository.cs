using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Entities;

namespace UMS.Application.Common.Interfaces
{
    public interface IEmailVerificationTokenRepository
    {
        Task AddAsync(EmailVerificationToken token, CancellationToken ct = default);
        Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken ct = default);
        Task UpdateAsync(EmailVerificationToken token, CancellationToken ct = default);
    }
}
