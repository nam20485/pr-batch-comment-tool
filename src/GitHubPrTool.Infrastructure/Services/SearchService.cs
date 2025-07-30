using System.Diagnostics;
using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using Microsoft.Extensions.Logging;

namespace GitHubPrTool.Infrastructure.Services;

/// <summary>
/// Service for searching across repositories, pull requests, and comments
/// </summary>
public class SearchService : ISearchService
{
    private readonly IGitHubRepository _repository;
    private readonly ILogger<SearchService> _logger;

    public SearchService(IGitHubRepository repository, ILogger<SearchService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SearchResults> SearchAsync(string query, SearchOptions? searchOptions = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new SearchResults { Query = query };
        }

        var stopwatch = Stopwatch.StartNew();
        searchOptions ??= new SearchOptions();

        _logger.LogInformation("Starting global search for query: '{Query}'", query);

        var tasks = new List<Task>();
        var repositories = new List<Repository>();
        var pullRequests = new List<PullRequest>();
        var comments = new List<Comment>();

        // Search repositories if enabled
        if (searchOptions.IncludeRepositories)
        {
            tasks.Add(Task.Run(async () =>
            {
                var results = await SearchRepositoriesAsync(query, cancellationToken);
                lock (repositories)
                {
                    repositories.AddRange(results.Take(searchOptions.MaxResultsPerCategory));
                }
            }, cancellationToken));
        }

        // Search pull requests if enabled
        if (searchOptions.IncludePullRequests)
        {
            tasks.Add(Task.Run(async () =>
            {
                var results = await SearchPullRequestsAsync(query, searchOptions.RepositoryId, cancellationToken);
                lock (pullRequests)
                {
                    pullRequests.AddRange(results.Take(searchOptions.MaxResultsPerCategory));
                }
            }, cancellationToken));
        }

        // Search comments if enabled
        if (searchOptions.IncludeComments)
        {
            tasks.Add(Task.Run(async () =>
            {
                var results = await SearchCommentsAsync(query, searchOptions.RepositoryId, null, cancellationToken);
                lock (comments)
                {
                    comments.AddRange(results.Take(searchOptions.MaxResultsPerCategory));
                }
            }, cancellationToken));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        var searchResults = new SearchResults
        {
            Query = query,
            Repositories = ApplyFilters(repositories, searchOptions),
            PullRequests = ApplyFilters(pullRequests, searchOptions),
            Comments = ApplyFilters(comments, searchOptions),
            ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
            IsTruncated = repositories.Count >= searchOptions.MaxResultsPerCategory ||
                         pullRequests.Count >= searchOptions.MaxResultsPerCategory ||
                         comments.Count >= searchOptions.MaxResultsPerCategory
        };

        _logger.LogInformation("Global search completed for query '{Query}' in {ElapsedMs}ms. Found {TotalResults} results",
            query, stopwatch.ElapsedMilliseconds, searchResults.TotalResults);

        return searchResults;
    }

    public async Task<IEnumerable<Repository>> SearchRepositoriesAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Enumerable.Empty<Repository>();
        }

        try
        {
            _logger.LogDebug("Searching repositories for query: '{Query}'", query);

            var allRepositories = await _repository.GetRepositoriesAsync(cancellationToken);
            
            var results = allRepositories.Where(repo =>
                ContainsQuery(repo.Name, query) ||
                ContainsQuery(repo.FullName, query) ||
                ContainsQuery(repo.Description, query) ||
                ContainsQuery(repo.Owner.Login, query) ||
                ContainsQuery(repo.Language, query)
            ).ToList();

            _logger.LogDebug("Found {Count} repositories matching query '{Query}'", results.Count, query);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching repositories for query '{Query}'", query);
            return Enumerable.Empty<Repository>();
        }
    }

    public async Task<IEnumerable<PullRequest>> SearchPullRequestsAsync(string query, long? repositoryId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Enumerable.Empty<PullRequest>();
        }

        try
        {
            _logger.LogDebug("Searching pull requests for query: '{Query}', RepositoryId: {RepositoryId}", query, repositoryId);

            IEnumerable<PullRequest> allPullRequests;
            
            if (repositoryId.HasValue)
            {
                allPullRequests = await _repository.GetPullRequestsAsync(repositoryId.Value, cancellationToken: cancellationToken);
            }
            else
            {
                // Get all repositories first, then get all pull requests for each
                var repositories = await _repository.GetRepositoriesAsync(cancellationToken);
                var pullRequestTasks = repositories.Select(repo => _repository.GetPullRequestsAsync(repo.Id, cancellationToken: cancellationToken));
                var pullRequestArrays = await Task.WhenAll(pullRequestTasks);
                allPullRequests = pullRequestArrays.SelectMany(prs => prs);
            }
            
            var results = allPullRequests.Where(pr =>
                ContainsQuery(pr.Title, query) ||
                ContainsQuery(pr.Body, query) ||
                ContainsQuery(pr.Author.Login, query) ||
                ContainsQuery(pr.BaseBranch, query) ||
                ContainsQuery(pr.HeadBranch, query) ||
                pr.Number.ToString().Contains(query)
            ).ToList();

            _logger.LogDebug("Found {Count} pull requests matching query '{Query}'", results.Count, query);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching pull requests for query '{Query}'", query);
            return Enumerable.Empty<PullRequest>();
        }
    }

    public async Task<IEnumerable<Comment>> SearchCommentsAsync(string query, long? repositoryId = null, long? pullRequestId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Enumerable.Empty<Comment>();
        }

        try
        {
            _logger.LogDebug("Searching comments for query: '{Query}', RepositoryId: {RepositoryId}, PullRequestId: {PullRequestId}", 
                query, repositoryId, pullRequestId);

            IEnumerable<Comment> allComments;

            if (pullRequestId.HasValue)
            {
                allComments = await _repository.GetCommentsAsync(pullRequestId.Value, cancellationToken);
            }
            else if (repositoryId.HasValue)
            {
                // Get all pull requests for the repository and then get all comments
                var pullRequests = await _repository.GetPullRequestsAsync(repositoryId.Value, state: null, cancellationToken);
                var commentTasks = pullRequests.Select(pr => _repository.GetCommentsAsync(pr.Id, cancellationToken));
                var commentArrays = await Task.WhenAll(commentTasks);
                allComments = commentArrays.SelectMany(c => c);
            }
            else
            {
                // Get all comments across all pull requests
                var repositories = await _repository.GetRepositoriesAsync(cancellationToken);
                var allPullRequestTasks = repositories.Select(repo => _repository.GetPullRequestsAsync(repo.Id, cancellationToken: cancellationToken));
                var allPullRequestArrays = await Task.WhenAll(allPullRequestTasks);
                var allPullRequests = allPullRequestArrays.SelectMany(prs => prs);
                
                var commentTasks = allPullRequests.Select(pr => _repository.GetCommentsAsync(pr.Id, cancellationToken));
                var commentArrays = await Task.WhenAll(commentTasks);
                allComments = commentArrays.SelectMany(c => c);
            }
            
            var results = allComments.Where(comment =>
                ContainsQuery(comment.Body, query) ||
                ContainsQuery(comment.Author.Login, query) ||
                ContainsQuery(comment.Path, query) ||
                ContainsQuery(comment.DiffHunk, query)
            ).ToList();

            _logger.LogDebug("Found {Count} comments matching query '{Query}'", results.Count, query);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching comments for query '{Query}'", query);
            return Enumerable.Empty<Comment>();
        }
    }

    public async Task<IEnumerable<string>> GetSearchSuggestionsAsync(string partialQuery, int maxSuggestions = 10, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(partialQuery) || partialQuery.Length < 2)
        {
            return Enumerable.Empty<string>();
        }

        try
        {
            _logger.LogDebug("Getting search suggestions for partial query: '{PartialQuery}'", partialQuery);

            var suggestions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Get suggestions from repositories
            var repositories = await _repository.GetRepositoriesAsync(cancellationToken);
            foreach (var repo in repositories)
            {
                AddSuggestion(suggestions, repo.Name, partialQuery);
                AddSuggestion(suggestions, repo.Owner.Login, partialQuery);
                if (!string.IsNullOrWhiteSpace(repo.Language))
                {
                    AddSuggestion(suggestions, repo.Language, partialQuery);
                }
            }

            // Get suggestions from pull requests (limited to avoid performance issues)
            var repoList = await _repository.GetRepositoriesAsync(cancellationToken);
            var pullRequestTasks = repoList.Take(10).Select(repo => _repository.GetPullRequestsAsync(repo.Id, state: null, cancellationToken)); // Limit repos for performance
            var pullRequestArrays = await Task.WhenAll(pullRequestTasks);
            var recentPullRequests = pullRequestArrays.SelectMany(prs => prs);
            
            foreach (var pr in recentPullRequests.Take(100)) // Limit for performance
            {
                AddSuggestion(suggestions, pr.Author.Login, partialQuery);
                AddSuggestion(suggestions, pr.BaseBranch, partialQuery);
                AddSuggestion(suggestions, pr.HeadBranch, partialQuery);
            }

            var result = suggestions.Take(maxSuggestions).ToList();
            _logger.LogDebug("Generated {Count} search suggestions for partial query '{PartialQuery}'", result.Count, partialQuery);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting search suggestions for partial query '{PartialQuery}'", partialQuery);
            return Enumerable.Empty<string>();
        }
    }

    private static bool ContainsQuery(string? text, string query)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return text.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private static void AddSuggestion(HashSet<string> suggestions, string? value, string partialQuery)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        if (value.StartsWith(partialQuery, StringComparison.OrdinalIgnoreCase))
        {
            suggestions.Add(value);
        }
    }

    private static IEnumerable<Repository> ApplyFilters(IEnumerable<Repository> repositories, SearchOptions options)
    {
        var filtered = repositories.AsEnumerable();

        if (options.Authors.Any())
        {
            filtered = filtered.Where(r => options.Authors.Contains(r.Owner.Login, StringComparer.OrdinalIgnoreCase));
        }

        if (options.DateFrom.HasValue)
        {
            filtered = filtered.Where(r => r.CreatedAt >= options.DateFrom.Value);
        }

        if (options.DateTo.HasValue)
        {
            filtered = filtered.Where(r => r.CreatedAt <= options.DateTo.Value);
        }

        return filtered;
    }

    private static IEnumerable<PullRequest> ApplyFilters(IEnumerable<PullRequest> pullRequests, SearchOptions options)
    {
        var filtered = pullRequests.AsEnumerable();

        if (options.Authors.Any())
        {
            filtered = filtered.Where(pr => options.Authors.Contains(pr.Author.Login, StringComparer.OrdinalIgnoreCase));
        }

        if (options.DateFrom.HasValue)
        {
            filtered = filtered.Where(pr => pr.CreatedAt >= options.DateFrom.Value);
        }

        if (options.DateTo.HasValue)
        {
            filtered = filtered.Where(pr => pr.CreatedAt <= options.DateTo.Value);
        }

        return filtered;
    }

    private static IEnumerable<Comment> ApplyFilters(IEnumerable<Comment> comments, SearchOptions options)
    {
        var filtered = comments.AsEnumerable();

        if (options.Authors.Any())
        {
            filtered = filtered.Where(c => options.Authors.Contains(c.Author.Login, StringComparer.OrdinalIgnoreCase));
        }

        if (options.DateFrom.HasValue)
        {
            filtered = filtered.Where(c => c.CreatedAt >= options.DateFrom.Value);
        }

        if (options.DateTo.HasValue)
        {
            filtered = filtered.Where(c => c.CreatedAt <= options.DateTo.Value);
        }

        return filtered;
    }
}