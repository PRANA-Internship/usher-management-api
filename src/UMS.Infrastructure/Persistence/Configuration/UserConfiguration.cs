using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Entities;
using UMS.Domain.Enums;

namespace UMS.Infrastructure.Persistance.Configuration
{
    public class Configuration : IEntityTypeConfiguration<User>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id)
                .HasColumnName("id");
            builder.Property(u => u.FullName)
                .HasColumnName("full_name")
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(u => u.Email)
                .HasColumnName("email").HasColumnType("citext")
                .IsRequired()
                .HasMaxLength(255);
            builder.Property(u => u.Phone)
                .HasColumnName("phone")
                .HasMaxLength(20)
                .IsRequired();
            builder.Property(u => u.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(255)
                .IsRequired(false);
            //  ^ NULL for guest only before email verification
            //it is constraineted in sql in the migration program
            //to ensure that only guest can have null password hash
            //and after email verification it must be set and not null
            //and in application logic
            builder.Property(u => u.Role)
                   .HasColumnName("role")
                   .HasConversion<string>()
                   .HasDefaultValue(UserRole.GUEST)
                   .IsRequired();
            builder.Property(u => u.Status)
                  .HasColumnName("status")
                  .HasConversion<string>()
                  .HasDefaultValue(UserStatus.ACTIVE)
                  .IsRequired();
            builder.Property(u => u.EmailVerified)
                  .HasColumnName("email_verified")
                  .HasDefaultValue(false)
                  .IsRequired();
            builder.Property(u => u.EmailVerifiedAt)
                  .HasColumnName("email_verified_at");
            builder.Property(u => u.RefreshToken)
                  .HasColumnName("refresh_token")
                  .HasMaxLength(255)
                  .IsRequired(false);
            builder.Property(u => u.RefreshTokenExpiry)
                  .HasColumnName("refresh_token_expiry")
                  .IsRequired(false);

            builder.Property(u => u.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd()
                  .IsRequired();
            builder.Property(u => u.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("NOW()")
                .IsRequired();
            builder.HasIndex(u => u.Email)
                   .IsUnique()
                   .HasDatabaseName("IDX_Users_Email");
            builder.HasOne(u => u.Usher)
                   .WithOne(ush => ush.User)
                   .HasForeignKey<Usher>(ush => ush.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.VerificationTokens)
                   .WithOne(t => t.User)
                   .HasForeignKey(t => t.UserId)
                   .OnDelete(DeleteBehavior.Cascade);


        }

    }
}
