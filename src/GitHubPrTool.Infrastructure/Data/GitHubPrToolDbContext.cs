using GitHubPrTool.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GitHubPrTool.Infrastructure.Data;

/// <summary>
/// Entity Framework DbContext for the GitHub PR Tool
/// </summary>
public class GitHubPrToolDbContext : DbContext
{
    public GitHubPrToolDbContext(DbContextOptions<GitHubPrToolDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Users table
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// Repositories table
    /// </summary>
    public DbSet<Repository> Repositories { get; set; } = null!;

    /// <summary>
    /// Pull requests table
    /// </summary>
    public DbSet<PullRequest> PullRequests { get; set; } = null!;

    /// <summary>
    /// Comments table
    /// </summary>
    public DbSet<Comment> Comments { get; set; } = null!;

    /// <summary>
    /// Reviews table
    /// </summary>
    public DbSet<Review> Reviews { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GitHubPrToolDbContext).Assembly);

        // Configure indexes for better query performance
        ConfigureIndexes(modelBuilder);

        // Configure enum conversions
        ConfigureEnums(modelBuilder);
    }

    private static void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // User indexes
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Login)
            .IsUnique();

        // Repository indexes
        modelBuilder.Entity<Repository>()
            .HasIndex(r => r.FullName)
            .IsUnique();

        // Pull Request indexes
        modelBuilder.Entity<PullRequest>()
            .HasIndex(pr => new { pr.RepositoryId, pr.Number })
            .IsUnique();

        modelBuilder.Entity<PullRequest>()
            .HasIndex(pr => pr.State);

        modelBuilder.Entity<PullRequest>()
            .HasIndex(pr => pr.CreatedAt);

        // Comment indexes
        modelBuilder.Entity<Comment>()
            .HasIndex(c => c.PullRequestId);

        modelBuilder.Entity<Comment>()
            .HasIndex(c => c.Type);

        modelBuilder.Entity<Comment>()
            .HasIndex(c => c.CreatedAt);

        modelBuilder.Entity<Comment>()
            .HasIndex(c => new { c.Path, c.Line })
            .HasFilter("Path IS NOT NULL AND Line IS NOT NULL");

        // Review indexes
        modelBuilder.Entity<Review>()
            .HasIndex(r => r.PullRequestId);

        modelBuilder.Entity<Review>()
            .HasIndex(r => r.State);

        modelBuilder.Entity<Review>()
            .HasIndex(r => r.SubmittedAt);
    }

    private static void ConfigureEnums(ModelBuilder modelBuilder)
    {
        // Configure enum to string conversions for better readability in database
        modelBuilder.Entity<PullRequest>()
            .Property(pr => pr.State)
            .HasConversion<string>();

        modelBuilder.Entity<Comment>()
            .Property(c => c.Type)
            .HasConversion<string>();

        modelBuilder.Entity<Review>()
            .Property(r => r.State)
            .HasConversion<string>();
    }
}