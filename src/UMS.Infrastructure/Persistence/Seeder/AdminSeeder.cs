using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Domain.Entities;
using UMS.Domain.Enums;
using UMS.Infrastructure.Persistance.Context;

namespace UMS.Infrastructure.Persistence.Seeder
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AdminSeederMarker>>();

            await SeedAdminAsync(db, passwordHasher, logger);
            await SeedCoordinatorAsync(db, passwordHasher, logger);
        }

        private static async Task SeedAdminAsync(
            AppDbContext db,
            IPasswordHasher passwordHasher,
            ILogger logger)
        {
            const string adminEmail = "ums@gmail.com";

            var exists = await db.Users.AnyAsync(u => u.Email == adminEmail);
            if (exists)
            {
                logger.LogInformation("Admin already exists — skipping seed.");
                return;
            }

            var passwordHash = passwordHasher.Hash("ums@4321");
            var admin = User.CreateUser("System Admin", adminEmail, "0911000000", passwordHash);
            admin.SetRole(UserRole.ADMIN);

            db.Users.Add(admin);
            await db.SaveChangesAsync();

            logger.LogInformation("Admin user seeded.");
        }

        private static async Task SeedCoordinatorAsync(
            AppDbContext db,
            IPasswordHasher passwordHasher,
            ILogger logger)
        {
            const string coordinatorEmail = "umscoordinator@gmail.com";

            var exists = await db.Users.AnyAsync(u => u.Email == coordinatorEmail);
            if (exists)
            {
                logger.LogInformation("Coordinator already exists — skipping seed.");
                return;
            }

            var passwordHash = passwordHasher.Hash("ums@4321");
            var coordinator = User.CreateUser(
                "Event Coordinator", coordinatorEmail, "0922000000", passwordHash);

            coordinator.SetRole(UserRole.EVENT_COORDINATOR);

            db.Users.Add(coordinator);
            await db.SaveChangesAsync();

            logger.LogInformation("Coordinator user seeded.");
        }

        public sealed class AdminSeederMarker { }
    }
}
