using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GitHubPrTool.Infrastructure.Data;

/// <summary>
/// Design-time factory for GitHubPrToolDbContext.
/// This is required for Entity Framework Core tools to work at design time.
/// </summary>
public class GitHubPrToolDbContextFactory : IDesignTimeDbContextFactory<GitHubPrToolDbContext>
{
    /// <summary>
    /// Creates a new instance of GitHubPrToolDbContext for design-time operations.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>Configured DbContext instance.</returns>
    public GitHubPrToolDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GitHubPrToolDbContext>();
        
        // Use SQLite for development/design-time
        // The connection string points to a local development database
        var developmentDbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GitHubPrTool",
            "Data",
            "githubprtool-dev.db"
        );
        
        // Ensure directory exists
        var directory = Path.GetDirectoryName(developmentDbPath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        optionsBuilder.UseSqlite($"Data Source={developmentDbPath}");
        
        return new GitHubPrToolDbContext(optionsBuilder.Options);
    }
}