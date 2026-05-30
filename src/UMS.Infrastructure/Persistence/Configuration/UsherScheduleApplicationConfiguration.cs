using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Entities;
using UMS.Domain.Enums;

namespace UMS.Infrastructure.Persistence.Configuration
{
    public class UsherScheduleApplicationConfiguration : IEntityTypeConfiguration<UsherScheduleApplication>
    {
        public void Configure(EntityTypeBuilder<UsherScheduleApplication> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.ExternalScheduleId).IsRequired().HasMaxLength(100);
            builder.Property(a => a.ExternalEventId).IsRequired().HasMaxLength(100);

            builder.Property(i => i.Status)
                .HasConversion(s => s.ToString(), s => Enum.Parse<InvitationStatus>(s));

            builder.HasIndex(a => new { a.ExternalScheduleId, a.UsherId })
                   .HasDatabaseName("IDX_UsherScheduleApplications_Schedule_Usher");

            builder.HasIndex(a => new { a.UsherId, a.Status, a.ScheduleStartDate, a.ScheduleEndDate })
                   .HasDatabaseName("IDX_UsherScheduleApplications_Availability");

            builder.HasOne(a => a.Usher)
                   .WithMany()
                   .HasForeignKey(a => a.UsherId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("UsherScheduleApplications");
        }
    }
}
