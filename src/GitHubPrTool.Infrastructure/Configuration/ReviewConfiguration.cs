using GitHubPrTool.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GitHubPrTool.Infrastructure.Configuration;

/// <summary>
/// Entity configuration for Review entity
/// </summary>
public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Body)
            .HasMaxLength(65000); // GitHub's maximum review body length

        builder.Property(r => r.HtmlUrl)
            .HasMaxLength(500);

        builder.Property(r => r.PullRequestUrl)
            .HasMaxLength(500);

        builder.Property(r => r.CommitId)
            .HasMaxLength(40); // SHA-1 hash length

        // Configure dates
        builder.Property(r => r.SubmittedAt)
            .HasColumnType("datetime");

        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime");

        builder.Property(r => r.UpdatedAt)
            .IsRequired()
            .HasColumnType("datetime");

        // Configure relationships
        builder.HasOne(r => r.Author)
            .WithMany()
            .HasForeignKey("AuthorId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.PullRequest)
            .WithMany(pr => pr.Reviews)
            .HasForeignKey(r => r.PullRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Comments)
            .WithOne(c => c.Review)
            .HasForeignKey(c => c.ReviewId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}