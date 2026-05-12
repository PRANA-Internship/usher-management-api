using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Entities;

namespace UMS.Infrastructure.Persistance.Configuration
{
    internal sealed class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
    {
        public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
        {
            builder.ToTable("EmailVerificationTokens");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Token)
                .HasColumnName("token")
                .IsRequired()
                .HasMaxLength(200);

            builder.HasIndex(t => t.Token)
                .IsUnique();

            builder.Property(t => t.TokenType)
                .HasColumnName("token_type")
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(t => t.ExpiresAt)
                .IsRequired();

            // Ignore computed properties — not stored in DB
            builder.Ignore(t => t.IsExpired);
            builder.Ignore(t => t.IsUsed);
            builder.Ignore(t => t.IsValid);
        }
    }
}
