using GitHubPrTool.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GitHubPrTool.Infrastructure.Configuration;

/// <summary>
/// Entity configuration for Comment entity
/// </summary>
public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Body)
            .IsRequired()
            .HasMaxLength(65000); // GitHub's maximum comment body length

        builder.Property(c => c.HtmlUrl)
            .HasMaxLength(500);

        builder.Property(c => c.Path)
            .HasMaxLength(500);

        builder.Property(c => c.CommitId)
            .HasMaxLength(40); // SHA-1 hash length

        builder.Property(c => c.OriginalCommitId)
            .HasMaxLength(40);

        builder.Property(c => c.DiffHunk)
            .HasMaxLength(10000);

        builder.Property(c => c.StartSide)
            .HasMaxLength(10);

        builder.Property(c => c.Side)
            .HasMaxLength(10);

        // Configure dates
        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime");

        builder.Property(c => c.UpdatedAt)
            .IsRequired()
            .HasColumnType("datetime");

        // Configure relationships
        builder.HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey("AuthorId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.PullRequest)
            .WithMany(pr => pr.Comments)
            .HasForeignKey(c => c.PullRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Review)
            .WithMany(r => r.Comments)
            .HasForeignKey(c => c.ReviewId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.InReplyTo)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.InReplyToId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}