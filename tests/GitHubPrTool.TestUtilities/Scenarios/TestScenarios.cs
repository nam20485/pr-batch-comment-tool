using GitHubPrTool.Core.Models;
using GitHubPrTool.TestUtilities.Builders;

namespace GitHubPrTool.TestUtilities.Scenarios;

/// <summary>
/// Provides realistic test scenarios for performance and integration testing
/// </summary>
public static class TestScenarios
{
    // Use a seeded random for reproducible test results
    private static readonly Random _random = new Random(42);
    /// <summary>
    /// Create a small-scale scenario (suitable for unit tests)
    /// </summary>
    public static TestScenario CreateSmallScenario()
    {
        var users = UserDataBuilder.CreateMany(5);
        var repositories = RepositoryDataBuilder.CreateMany(2);
        var pullRequests = PullRequestDataBuilder.CreateMany(3);
        var comments = CommentDataBuilder.CreateMany(15);
        var reviews = ReviewDataBuilder.CreateMany(6);

        return new TestScenario
        {
            Name = "Small",
            Users = users,
            Repositories = repositories,
            PullRequests = pullRequests,
            Comments = comments,
            Reviews = reviews
        };
    }

    /// <summary>
    /// Create a medium-scale scenario (suitable for integration tests)
    /// </summary>
    public static TestScenario CreateMediumScenario()
    {
        var users = UserDataBuilder.CreateMany(25);
        var repositories = RepositoryDataBuilder.CreateMany(10);
        var pullRequests = PullRequestDataBuilder.CreateMany(50);
        var comments = CommentDataBuilder.CreateMany(200);
        var reviews = ReviewDataBuilder.CreateMany(75);

        return new TestScenario
        {
            Name = "Medium",
            Users = users,
            Repositories = repositories,
            PullRequests = pullRequests,
            Comments = comments,
            Reviews = reviews
        };
    }

    /// <summary>
    /// Create a large-scale scenario (suitable for performance tests)
    /// </summary>
    public static TestScenario CreateLargeScenario()
    {
        var users = UserDataBuilder.CreateMany(100);
        var repositories = RepositoryDataBuilder.CreateMany(50);
        var pullRequests = PullRequestDataBuilder.CreateMany(500);
        var comments = CommentDataBuilder.CreateMany(2000);
        var reviews = ReviewDataBuilder.CreateMany(1000);

        return new TestScenario
        {
            Name = "Large",
            Users = users,
            Repositories = repositories,
            PullRequests = pullRequests,
            Comments = comments,
            Reviews = reviews
        };
    }

    /// <summary>
    /// Create an enterprise-scale scenario (suitable for stress tests)
    /// </summary>
    public static TestScenario CreateEnterpriseScenario()
    {
        var users = UserDataBuilder.CreateMany(1000);
        var repositories = RepositoryDataBuilder.CreateMany(200);
        var pullRequests = PullRequestDataBuilder.CreateMany(5000);
        var comments = CommentDataBuilder.CreateMany(25000);
        var reviews = ReviewDataBuilder.CreateMany(10000);

        return new TestScenario
        {
            Name = "Enterprise",
            Users = users,
            Repositories = repositories,
            PullRequests = pullRequests,
            Comments = comments,
            Reviews = reviews
        };
    }

    /// <summary>
    /// Create a realistic repository with full comment and review activity
    /// </summary>
    public static Repository CreateActiveRepository()
    {
        var repository = RepositoryDataBuilder.Create();
        var pullRequests = new List<PullRequest>();

        // Create multiple PRs with varied activity levels
        for (int i = 0; i < 10; i++)
        {
            var pr = PullRequestDataBuilder.CreateOpen();
            pr.RepositoryId = repository.Id;
            pr.Repository = repository;

            // Add comments to PR
            var comments = CommentDataBuilder.CreateManyForPullRequest(pr.Id, _random.Next(1, 15));
            pr.Comments = comments.ToList();

            // Add reviews to PR
            var reviews = ReviewDataBuilder.CreateManyForPullRequest(pr.Id, _random.Next(1, 5));
            pr.Reviews = reviews.ToList();

            // Add review comments
            foreach (var review in reviews)
            {
                var reviewComments = CommentDataBuilder.CreateManyForPullRequest(pr.Id, _random.Next(0, 3))
                    .Select(c => { c.ReviewId = review.Id; c.Review = review; c.Type = CommentType.Review; return c; })
                    .ToList();
                review.Comments = reviewComments;
            }

            pullRequests.Add(pr);
        }

        repository.PullRequests = pullRequests;
        return repository;
    }

    /// <summary>
    /// Create a repository with heavy comment activity (for performance testing)
    /// </summary>
    public static Repository CreateHighVolumeRepository()
    {
        var repository = RepositoryDataBuilder.Create();
        var pullRequests = new List<PullRequest>();

        // Create PRs with high comment volumes
        for (int i = 0; i < 50; i++)
        {
            var pr = PullRequestDataBuilder.CreateOpen();
            pr.RepositoryId = repository.Id;
            pr.Repository = repository;

            // High volume of comments per PR
            var comments = CommentDataBuilder.CreateManyForPullRequest(pr.Id, _random.Next(50, 200));
            pr.Comments = comments.ToList();

            // Many reviews per PR
            var reviews = ReviewDataBuilder.CreateManyForPullRequest(pr.Id, _random.Next(10, 25));
            pr.Reviews = reviews.ToList();

            pullRequests.Add(pr);
        }

        repository.PullRequests = pullRequests;
        return repository;
    }

    /// <summary>
    /// Create test data for comment filtering scenarios
    /// </summary>
    public static IList<Comment> CreateFilteringTestComments()
    {
        var comments = new List<Comment>();
        var users = UserDataBuilder.CreateMany(5);

        // Comments from specific authors
        comments.AddRange(CommentDataBuilder.CreateMany(10)
            .Select((c, i) => { c.Author = users[i % users.Count]; return c; }));

        // Comments with specific content patterns
        comments.AddRange(CommentDataBuilder.WithBodyContaining("bug", 5));
        comments.AddRange(CommentDataBuilder.WithBodyContaining("feature", 3));
        comments.AddRange(CommentDataBuilder.WithBodyContaining("refactor", 2));

        // Comments of different types
        comments.AddRange(Enumerable.Range(0, 5).Select(_ => CommentDataBuilder.CreateIssueComment()));
        comments.AddRange(Enumerable.Range(0, 8).Select(_ => CommentDataBuilder.CreateReviewComment()));
        comments.AddRange(Enumerable.Range(0, 3).Select(_ => CommentDataBuilder.CreateCommitComment()));

        // Comments in different date ranges
        var now = DateTimeOffset.Now;
        comments.AddRange(CommentDataBuilder.CreateInDateRange(now.AddDays(-7), now.AddDays(-1), 10));
        comments.AddRange(CommentDataBuilder.CreateInDateRange(now.AddDays(-30), now.AddDays(-7), 15));

        return comments;
    }
}

/// <summary>
/// Represents a complete test scenario with related entities
/// </summary>
public class TestScenario
{
    public string Name { get; set; } = string.Empty;
    public IList<User> Users { get; set; } = new List<User>();
    public IList<Repository> Repositories { get; set; } = new List<Repository>();
    public IList<PullRequest> PullRequests { get; set; } = new List<PullRequest>();
    public IList<Comment> Comments { get; set; } = new List<Comment>();
    public IList<Review> Reviews { get; set; } = new List<Review>();

    public int TotalEntities => Users.Count + Repositories.Count + PullRequests.Count + Comments.Count + Reviews.Count;
}