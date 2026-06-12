using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using UMS.Domain.Entities;

namespace UMS.Infrastructure.Persistence.Repositories
{

    public sealed class UsherPerformanceReviewConfiguration
        : IEntityTypeConfiguration<UsherPerformanceReview>
    {
        public void Configure(EntityTypeBuilder<UsherPerformanceReview> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.ExternalScheduleId).IsRequired().HasMaxLength(100);
            builder.Property(r => r.ExternalEventId).IsRequired().HasMaxLength(100);
            builder.Property(r => r.Remarks).HasMaxLength(1000);
            builder.HasIndex(r => new { r.ExternalScheduleId, r.UsherId })
                   .IsUnique()
                   .HasDatabaseName("IDX_PerformanceReview_Schedule_Usher");


            builder.Ignore(r => r.AverageRating);

            builder.HasOne(r => r.Usher)
                   .WithMany()
                   .HasForeignKey(r => r.UsherId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("UsherPerformanceReviews");
        }
    }
}