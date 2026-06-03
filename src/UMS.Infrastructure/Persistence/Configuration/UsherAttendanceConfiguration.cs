using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Enums;
using UMS.Domain.Entities;

namespace UMS.Infrastructure.Persistence.Configuration
{
    public sealed class UsherAttendanceConfiguration
      : IEntityTypeConfiguration<UsherAttendance>
    {
        public void Configure(EntityTypeBuilder<UsherAttendance> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.ExternalScheduleId).IsRequired().HasMaxLength(100);
            builder.Property(a => a.ExternalEventId).IsRequired().HasMaxLength(100);

            builder.Property(a => a.DayStatus)
                .HasConversion(
                    d => d.ToString(),
                    d => Enum.Parse<DayStatus>(d));

            builder.Property(a => a.Status)
                .HasConversion(
                    s => (int)s,
                    s => (AttendanceStatus)s);
            builder.HasIndex(a => new
            {
                a.ExternalScheduleId,
                a.UsherId,
                a.AttendanceDate,
                a.DayStatus
            })
            .IsUnique()
            .HasDatabaseName("IDX_Attendance_Schedule_Usher_Date_Session");

            builder.HasIndex(a => new
            {
                a.ExternalScheduleId,
                a.AttendanceDate,
                a.DayStatus
            })
            .HasDatabaseName("IDX_Attendance_Schedule_Date_Session");

            builder.HasOne(a => a.Usher)
                   .WithMany()
                   .HasForeignKey(a => a.UsherId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("UsherAttendances");
        }
    }

}
