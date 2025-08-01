# API Reference - GitHub PR Review Tool

This document provides comprehensive API documentation for the GitHub PR Review Tool's internal architecture and extensibility points.

## Table of Contents

1. [Core Interfaces](#core-interfaces)
2. [Domain Models](#domain-models)
3. [Service Interfaces](#service-interfaces)
4. [Repository Pattern](#repository-pattern)
5. [ViewModels](#viewmodels)
6. [Events and Messaging](#events-and-messaging)
7. [Configuration](#configuration)
8. [Extension Points](#extension-points)

## Core Interfaces

### IRepository<T>

Generic repository interface for data access operations.

```csharp
namespace GitHubPrTool.Core.Interfaces;

/// <summary>
/// Generic repository interface for data access operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets an entity by its identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns>The entity if found, otherwise null.</returns>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Gets all entities of type T.
    /// </summary>
    /// <returns>A collection of all entities.</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>The added entity with generated identifier.</returns>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Deletes an entity by identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    Task DeleteAsync(int id);

    /// <summary>
    /// Checks if an entity exists by identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns>True if the entity exists, otherwise false.</returns>
    Task<bool> ExistsAsync(int id);
}
```

### IGitHubService

Interface for GitHub API integration.

```csharp
namespace GitHubPrTool.Core.Interfaces;

/// <summary>
/// Provides GitHub API integration services.
/// </summary>
public interface IGitHubService
{
    /// <summary>
    /// Gets the current authentication status.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Authenticates with GitHub using OAuth flow.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if authentication succeeded.</returns>
    Task<bool> AuthenticateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets repositories accessible to the authenticated user.
    /// </summary>
    /// <param name="filter">Optional repository filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of repositories.</returns>
    Task<IEnumerable<Repository>> GetRepositoriesAsync(
        RepositoryFilter? filter = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pull requests for a specific repository.
    /// </summary>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <param name="filter">Optional pull request filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of pull requests.</returns>
    Task<IEnumerable<PullRequest>> GetPullRequestsAsync(
        string owner, 
        string repo, 
        PullRequestFilter? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets comments for a specific pull request.
    /// </summary>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <param name="pullNumber">Pull request number.</param>
    /// <param name="filter">Optional comment filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of comments.</returns>
    Task<IEnumerable<Comment>> GetCommentsAsync(
        string owner, 
        string repo, 
        int pullNumber,
        CommentFilter? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new comment on a pull request.
    /// </summary>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <param name="pullNumber">Pull request number.</param>
    /// <param name="body">Comment body.</param>
    /// <param name="path">Optional file path for line comments.</param>
    /// <param name="line">Optional line number for line comments.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created comment.</returns>
    Task<Comment> CreateCommentAsync(
        string owner, 
        string repo, 
        int pullNumber, 
        string body,
        string? path = null,
        int? line = null,
        CancellationToken cancellationToken = default);
}
```

## Domain Models

### Repository

```csharp
namespace GitHubPrTool.Core.Entities;

/// <summary>
/// Represents a GitHub repository.
/// </summary>
public class Repository
{
    /// <summary>
    /// Gets or sets the repository identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the repository name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository owner.
    /// </summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the primary programming language.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Gets or sets the star count.
    /// </summary>
    public int Stars { get; set; }

    /// <summary>
    /// Gets or sets whether the repository is private.
    /// </summary>
    public bool IsPrivate { get; set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update date.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the repository URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of pull requests.
    /// </summary>
    public ICollection<PullRequest> PullRequests { get; set; } = new List<PullRequest>();
}
```

### PullRequest

```csharp
namespace GitHubPrTool.Core.Entities;

/// <summary>
/// Represents a GitHub pull request.
/// </summary>
public class PullRequest
{
    /// <summary>
    /// Gets or sets the pull request identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the pull request number.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Gets or sets the pull request title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pull request description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the author username.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pull request state.
    /// </summary>
    public PullRequestState State { get; set; }

    /// <summary>
    /// Gets or sets whether this is a draft pull request.
    /// </summary>
    public bool IsDraft { get; set; }

    /// <summary>
    /// Gets or sets the source branch.
    /// </summary>
    public string SourceBranch { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target branch.
    /// </summary>
    public string TargetBranch { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update date.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the merge date if merged.
    /// </summary>
    public DateTime? MergedAt { get; set; }

    /// <summary>
    /// Gets or sets the repository identifier.
    /// </summary>
    public int RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets the associated repository.
    /// </summary>
    public Repository Repository { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of comments.
    /// </summary>
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}

/// <summary>
/// Represents pull request states.
/// </summary>
public enum PullRequestState
{
    Open,
    Closed,
    Merged
}
```

### Comment

```csharp
namespace GitHubPrTool.Core.Entities;

/// <summary>
/// Represents a pull request comment.
/// </summary>
public class Comment
{
    /// <summary>
    /// Gets or sets the comment identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the comment body.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author username.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file path for line comments.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets the line number for line comments.
    /// </summary>
    public int? Line { get; set; }

    /// <summary>
    /// Gets or sets whether the comment is outdated.
    /// </summary>
    public bool IsOutdated { get; set; }

    /// <summary>
    /// Gets or sets whether the comment is resolved.
    /// </summary>
    public bool IsResolved { get; set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update date.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the pull request identifier.
    /// </summary>
    public int PullRequestId { get; set; }

    /// <summary>
    /// Gets or sets the associated pull request.
    /// </summary>
    public PullRequest PullRequest { get; set; } = null!;
}
```

## Service Interfaces

### ISyncService

```csharp
namespace GitHubPrTool.Core.Interfaces;

/// <summary>
/// Provides data synchronization services between local cache and GitHub API.
/// </summary>
public interface ISyncService
{
    /// <summary>
    /// Gets the last synchronization timestamp.
    /// </summary>
    DateTime? LastSyncTime { get; }

    /// <summary>
    /// Gets whether a sync operation is currently in progress.
    /// </summary>
    bool IsSyncing { get; }

    /// <summary>
    /// Synchronizes all data with GitHub.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SyncAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes a specific repository.
    /// </summary>
    /// <param name="repositoryId">Repository identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SyncRepositoryAsync(int repositoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes a specific pull request.
    /// </summary>
    /// <param name="pullRequestId">Pull request identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SyncPullRequestAsync(int pullRequestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Event raised when synchronization progress changes.
    /// </summary>
    event EventHandler<SyncProgressEventArgs> SyncProgressChanged;

    /// <summary>
    /// Event raised when synchronization completes.
    /// </summary>
    event EventHandler<SyncCompletedEventArgs> SyncCompleted;
}
```

### ITokenStorage

```csharp
namespace GitHubPrTool.Core.Interfaces;

/// <summary>
/// Provides secure token storage services.
/// </summary>
public interface ITokenStorage
{
    /// <summary>
    /// Stores an authentication token securely.
    /// </summary>
    /// <param name="token">The token to store.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task StoreTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the stored authentication token.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The stored token, or null if not found.</returns>
    Task<string?> GetTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the stored authentication token.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a token is currently stored.
    /// </summary>
    /// <returns>True if a token is stored, otherwise false.</returns>
    Task<bool> HasTokenAsync();
}
```

## Repository Pattern

### Base Repository Implementation

```csharp
namespace GitHubPrTool.Infrastructure.Data.Repositories;

/// <summary>
/// Base repository implementation with common functionality.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public abstract class RepositoryBase<T> : IRepository<T> where T : class
{
    protected readonly GitHubPrToolDbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected RepositoryBase(GitHubPrToolDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        var result = await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return result.Entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.FindAsync(id) != null;
    }
}
```

## ViewModels

### Base ViewModel

```csharp
namespace GitHubPrTool.Desktop.ViewModels;

/// <summary>
/// Base view model with common MVVM functionality.
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
    private bool _isBusy;
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Gets or sets whether the view model is currently busy.
    /// </summary>
    [ObservableProperty]
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    /// <summary>
    /// Gets or sets the current status message.
    /// </summary>
    [ObservableProperty]
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>
    /// Executes an async operation with busy state management.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="statusMessage">Optional status message.</param>
    protected async Task ExecuteAsync(Func<Task> operation, string? statusMessage = null)
    {
        try
        {
            IsBusy = true;
            if (!string.IsNullOrEmpty(statusMessage))
            {
                StatusMessage = statusMessage;
            }

            await operation();
        }
        finally
        {
            IsBusy = false;
            StatusMessage = string.Empty;
        }
    }
}
```

## Events and Messaging

### Message Types

```csharp
namespace GitHubPrTool.Desktop.Messages;

/// <summary>
/// Message sent when a repository is selected.
/// </summary>
public class RepositorySelectedMessage
{
    public Repository Repository { get; }

    public RepositorySelectedMessage(Repository repository)
    {
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }
}

/// <summary>
/// Message sent when a pull request is selected.
/// </summary>
public class PullRequestSelectedMessage
{
    public PullRequest PullRequest { get; }

    public PullRequestSelectedMessage(PullRequest pullRequest)
    {
        PullRequest = pullRequest ?? throw new ArgumentNullException(nameof(pullRequest));
    }
}

/// <summary>
/// Message sent when synchronization starts or completes.
/// </summary>
public class SyncStatusChangedMessage
{
    public bool IsSync { get; }
    public string? Message { get; }

    public SyncStatusChangedMessage(bool isSync, string? message = null)
    {
        IsSync = isSync;
        Message = message;
    }
}
```

## Configuration

### Application Settings

```csharp
namespace GitHubPrTool.Core.Configuration;

/// <summary>
/// Application configuration settings.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Gets or sets GitHub API configuration.
    /// </summary>
    public GitHubSettings GitHub { get; set; } = new();

    /// <summary>
    /// Gets or sets database configuration.
    /// </summary>
    public DatabaseSettings Database { get; set; } = new();

    /// <summary>
    /// Gets or sets UI configuration.
    /// </summary>
    public UISettings UI { get; set; } = new();

    /// <summary>
    /// Gets or sets synchronization configuration.
    /// </summary>
    public SyncSettings Sync { get; set; } = new();
}

/// <summary>
/// GitHub API configuration.
/// </summary>
public class GitHubSettings
{
    /// <summary>
    /// Gets or sets the GitHub API base URL.
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://api.github.com";

    /// <summary>
    /// Gets or sets the OAuth client ID.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the OAuth client secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the number of retry attempts.
    /// </summary>
    public int RetryAttempts { get; set; } = 3;
}
```

## Extension Points

### Custom Filters

```csharp
namespace GitHubPrTool.Core.Interfaces;

/// <summary>
/// Interface for custom comment filters.
/// </summary>
public interface ICommentFilter
{
    /// <summary>
    /// Gets the filter name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the filter description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Applies the filter to a collection of comments.
    /// </summary>
    /// <param name="comments">The comments to filter.</param>
    /// <returns>The filtered comments.</returns>
    IEnumerable<Comment> Apply(IEnumerable<Comment> comments);
}
```

### Custom Exporters

```csharp
namespace GitHubPrTool.Core.Interfaces;

/// <summary>
/// Interface for custom data exporters.
/// </summary>
public interface IDataExporter
{
    /// <summary>
    /// Gets the exporter name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets supported file extensions.
    /// </summary>
    IEnumerable<string> SupportedExtensions { get; }

    /// <summary>
    /// Exports comments to the specified format.
    /// </summary>
    /// <param name="comments">The comments to export.</param>
    /// <param name="filePath">The output file path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ExportAsync(IEnumerable<Comment> comments, string filePath, CancellationToken cancellationToken = default);
}
```

---

For more detailed implementation examples and usage patterns, see the [Developer Guide](developer-guide.md) and source code documentation.