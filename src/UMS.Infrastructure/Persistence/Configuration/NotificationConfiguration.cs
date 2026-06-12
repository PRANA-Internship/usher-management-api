using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using UMS.Domain.Entities;
using UMS.Domain.Enums;

namespace UMS.Infrastructure.Persistence.Configuration
{

    public sealed class NotificationConfiguration
        : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.Id);

            builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
            builder.Property(n => n.Message).IsRequired().HasMaxLength(500);
            builder.Property(n => n.Payload).HasMaxLength(1000);

            builder.Property(n => n.Type)
                .HasConversion(
                    t => t.ToString(),
                    t => Enum.Parse<NotificationType>(t));

            builder.HasIndex(n => new { n.RecipientId, n.IsRead, n.CreatedAt })
                   .HasDatabaseName("IDX_Notifications_Recipient_Read_Date");

            builder.ToTable("Notifications");
        }
    }
}