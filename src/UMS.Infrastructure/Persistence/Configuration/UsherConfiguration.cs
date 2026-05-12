using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Entities;
using UMS.Domain.Enums;

namespace UMS.Infrastructure.Persistance.Configuration
{
    public class UsherConfiguration : IEntityTypeConfiguration<Usher>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Usher> builder)
        {
            builder.ToTable("ushers");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id)
                .HasColumnName("id");
            builder.Property(u => u.UserId)
                  .HasColumnName("user_id")
                  .IsRequired();
            builder.Property(u => u.Gender)
                .HasColumnName("gender")
                .HasConversion<string>()
                .HasMaxLength(6)
                .IsRequired();
            builder.Property(u => u.DateOfBirth)
                .HasColumnName("date_of_birth")
                .HasColumnType("date")
                .IsRequired();
            builder.Property(u => u.Address)
                .HasColumnName("address")
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(u => u.EmergencyContactName)
                .HasColumnName("emergency_contact_name")
                .HasMaxLength(100)
                .IsRequired();
            builder.Property(u => u.EmergencyContactPhone)
                  .HasColumnName("emergency_contact_phone")
                  .HasMaxLength(20)
                  .IsRequired();
            builder.Property(u => u.EducationLevel)
                .HasColumnName("education_level")
                .HasMaxLength(25)
                .IsRequired();
            builder.Property(u => u.ExperienceSummary)
                .HasColumnName("experience_summary")
                .HasMaxLength(500)
                .IsRequired();
            builder.Property(u => u.Languages)
                .HasColumnName("languages")
                .HasMaxLength(100)
                .IsRequired();
            builder.Property(u => u.Sector)
                .HasColumnName("sector")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(u => u.ProfilePhotoUrl)
                .HasColumnName("profile_photo_url")
                .IsRequired();

            builder.Property(u => u.IdDocumentUrl)
                .HasColumnName("id_document_url")
                .IsRequired();

            builder.Property(u => u.ApprovalStatus)
                .HasColumnName("approval_status")
                .HasConversion<string>()
                .HasDefaultValue(ApprovalStatus.PENDING)
                .IsRequired();

            builder.Property(u => u.ApprovedBy)
                .HasColumnName("approved_by");

            builder.Property(u => u.ApprovedAt)
                .HasColumnName("approved_at");
            builder.Property(u => u.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()")
                .IsRequired();

            builder.Property(u => u.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("NOW()")
                .IsRequired();
            builder.HasOne(u => u.User)
                .WithOne(u => u.Usher)
                .HasForeignKey<Usher>(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(u => u.ApprovedByUser)
                .WithMany()
                .HasForeignKey(u => u.ApprovedBy)
                .OnDelete(DeleteBehavior.SetNull);
            builder.HasIndex(u => u.UserId)
                   .IsUnique()
                   .HasDatabaseName("uq_ushers_user_id");

            builder.HasIndex(u => u.ApprovalStatus)
                   .HasFilter("approval_status = 'PENDING'")
                   .HasDatabaseName("idx_ushers_approval_status");

        }

    }

}
