using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Domain.Entities;
using UMS.Infrastructure.Persistance.Context;

namespace UMS.Infrastructure.Persistence.Repositories
{

    public sealed class UsherRepository(AppDbContext db) : IUsherRepository
    {
        public async Task AddAsync(Usher usher, CancellationToken ct = default) =>
            await db.Ushers.AddAsync(usher, ct);

        public async Task<Usher?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
          await db.Ushers.FirstOrDefaultAsync(u => u.UserId == userId, ct);
    }

}
