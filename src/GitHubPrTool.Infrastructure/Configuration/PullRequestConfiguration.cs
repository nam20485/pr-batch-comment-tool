using GitHubPrTool.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GitHubPrTool.Infrastructure.Configuration;

/// <summary>
/// Entity configuration for PullRequest entity
/// </summary>
public class PullRequestConfiguration : IEntityTypeConfiguration<PullRequest>
{
    public void Configure(EntityTypeBuilder<PullRequest> builder)
    {
        builder.ToTable("PullRequests");

        builder.HasKey(pr => pr.Id);

        builder.Property(pr => pr.Number)
            .IsRequired();

        builder.Property(pr => pr.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(pr => pr.Body)
            .HasMaxLength(65000); // GitHub's maximum issue body length

        builder.Property(pr => pr.HtmlUrl)
            .HasMaxLength(500);

        builder.Property(pr => pr.BaseBranch)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pr => pr.HeadBranch)
            .IsRequired()
            .HasMaxLength(200);

        // Configure dates
        builder.Property(pr => pr.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime");

        builder.Property(pr => pr.UpdatedAt)
            .IsRequired()
            .HasColumnType("datetime");

        builder.Property(pr => pr.ClosedAt)
            .HasColumnType("datetime");

        builder.Property(pr => pr.MergedAt)
            .HasColumnType("datetime");

        // Configure relationships
        builder.HasOne(pr => pr.Author)
            .WithMany()
            .HasForeignKey("AuthorId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pr => pr.Repository)
            .WithMany(r => r.PullRequests)
            .HasForeignKey(pr => pr.RepositoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pr => pr.MergedBy)
            .WithMany()
            .HasForeignKey("MergedById")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(pr => pr.Reviews)
            .WithOne(r => r.PullRequest)
            .HasForeignKey(r => r.PullRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(pr => pr.Comments)
            .WithOne(c => c.PullRequest)
            .HasForeignKey(c => c.PullRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}