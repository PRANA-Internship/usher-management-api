using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using UMS.Domain.Entities;

namespace UMS.Infrastructure.Persistence.Configuration
{
    public sealed class ScheduleAssignmentConfiguration
    : IEntityTypeConfiguration<ScheduleAssignment>
    {
        public void Configure(EntityTypeBuilder<ScheduleAssignment> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.ExternalScheduleId).IsRequired().HasMaxLength(100);
            builder.Property(a => a.ExternalEventId).IsRequired().HasMaxLength(100);

            builder.HasIndex(a => a.ExternalScheduleId)
                   .IsUnique()
                   .HasDatabaseName("IDX_ScheduleAssignments_ExternalScheduleId");

            builder.HasOne(a => a.Coordinator)
                   .WithMany()
                   .HasForeignKey(a => a.CoordinatorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.AssignedBy)
                   .WithMany()
                   .HasForeignKey(a => a.AssignedByAdminId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("ScheduleAssignments");
        }
    }
}