using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Entities;
using UMS.Domain.Enums;

namespace UMS.Infrastructure.Persistence.Configuration
{
    public sealed class UsherInvitationConfiguration
    : IEntityTypeConfiguration<UsherInvitation>
    {
        public void Configure(EntityTypeBuilder<UsherInvitation> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.ExternalScheduleId).IsRequired().HasMaxLength(100);
            builder.Property(i => i.ExternalEventId).IsRequired().HasMaxLength(100);

            builder.Property(i => i.Status)
                .HasConversion(s => s.ToString(), s => Enum.Parse<InvitationStatus>(s));

            builder.HasIndex(i => new { i.ExternalScheduleId, i.UsherId })
                   .HasDatabaseName("IDX_UsherInvitations_Schedule_Usher");

            builder.HasIndex(i => new { i.UsherId, i.Status, i.ScheduleStartDate, i.ScheduleEndDate })
                   .HasDatabaseName("IDX_UsherInvitations_Availability");

            builder.HasOne(i => i.Usher)
                   .WithMany()
                   .HasForeignKey(i => i.UsherId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.InvitedByCoordinator)
                   .WithMany()
                   .HasForeignKey(i => i.InvitedByCoordinatorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("UsherInvitations");
        }
    }

}
