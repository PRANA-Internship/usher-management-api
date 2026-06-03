using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UMS.Domain.Entities;

namespace UMS.Infrastructure.Persistance.Context
{
    public sealed class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Usher> Ushers => Set<Usher>();
        public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
        public DbSet<ScheduleAssignment> ScheduleAssignments => Set<ScheduleAssignment>();
        public DbSet<UsherInvitation> UsherInvitations => Set<UsherInvitation>();
        public DbSet<UsherScheduleApplication> UsherScheduleApplications => Set<UsherScheduleApplication>();
        public DbSet<UsherAttendance> UsherAttendances => Set<UsherAttendance>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            modelBuilder.HasPostgresExtension("pgcrypto");
            modelBuilder.HasPostgresExtension("citext");


            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }


    }
}
