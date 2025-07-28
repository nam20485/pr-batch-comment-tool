using GitHubPrTool.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GitHubPrTool.Infrastructure.Configuration;

/// <summary>
/// Entity configuration for Repository entity
/// </summary>
public class RepositoryConfiguration : IEntityTypeConfiguration<Repository>
{
    public void Configure(EntityTypeBuilder<Repository> builder)
    {
        builder.ToTable("Repositories");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .HasMaxLength(1000);

        builder.Property(r => r.HtmlUrl)
            .HasMaxLength(500);

        builder.Property(r => r.CloneUrl)
            .HasMaxLength(500);

        builder.Property(r => r.DefaultBranch)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Language)
            .HasMaxLength(50);

        // Configure dates
        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime");

        builder.Property(r => r.UpdatedAt)
            .IsRequired()
            .HasColumnType("datetime");

        builder.Property(r => r.PushedAt)
            .HasColumnType("datetime");

        // Configure relationships
        builder.HasOne(r => r.Owner)
            .WithMany()
            .HasForeignKey("OwnerId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.PullRequests)
            .WithOne(pr => pr.Repository)
            .HasForeignKey(pr => pr.RepositoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}