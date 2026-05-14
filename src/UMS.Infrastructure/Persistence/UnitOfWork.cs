using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Infrastructure.Persistance.Context;

namespace UMS.Infrastructure.Persistence
{

    public sealed class UnitOfWork(AppDbContext db) : IUnitOfWork
    {
        public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken ct = default)
        {
            var strategy = db.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await db.Database.BeginTransactionAsync(ct);
                try
                {
                    await operation();
                    await db.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);
                }
                catch
                {
                    await transaction.RollbackAsync(ct);
                    throw;
                }
            });
        }
    }
}