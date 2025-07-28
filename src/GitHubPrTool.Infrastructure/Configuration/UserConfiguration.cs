using GitHubPrTool.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GitHubPrTool.Infrastructure.Configuration;

/// <summary>
/// Entity configuration for User entity
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Login)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Name)
            .HasMaxLength(200);

        builder.Property(u => u.Email)
            .HasMaxLength(320); // RFC 5321 maximum email length

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(u => u.HtmlUrl)
            .HasMaxLength(500);

        builder.Property(u => u.Bio)
            .HasMaxLength(1000);

        // Configure dates
        builder.Property(u => u.CreatedAt)
            .HasColumnType("datetime");

        builder.Property(u => u.UpdatedAt)
            .HasColumnType("datetime");
    }
}