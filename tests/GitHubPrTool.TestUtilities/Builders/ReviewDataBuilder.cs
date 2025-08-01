using Bogus;
using GitHubPrTool.Core.Models;

namespace GitHubPrTool.TestUtilities.Builders;

/// <summary>
/// Test data generator for Review models using Bogus
/// </summary>
public static class ReviewDataBuilder
{
    private static readonly Faker<Review> _faker = new Faker<Review>()
        .RuleFor(r => r.Id, f => f.Random.Long(1, 1000000))
        .RuleFor(r => r.Body, f => f.Lorem.Paragraphs())
        .RuleFor(r => r.State, f => f.PickRandom<ReviewState>())
        .RuleFor(r => r.Author, f => UserDataBuilder.CreateMinimal())
        .RuleFor(r => r.PullRequest, f => new PullRequest { Id = f.Random.Long(1, 1000000) })
        .RuleFor(r => r.PullRequestId, (f, r) => r.PullRequest.Id)
        .RuleFor(r => r.HtmlUrl, f => f.Internet.Url())
        .RuleFor(r => r.PullRequestUrl, f => f.Internet.Url())
        .RuleFor(r => r.CommitId, f => f.Random.Hash(40))
        .RuleFor(r => r.CreatedAt, f => f.Date.PastOffset(30))
        .RuleFor(r => r.UpdatedAt, f => f.Date.RecentOffset())
        .RuleFor(r => r.SubmittedAt, f => f.Date.PastOffset(30))
        .RuleFor(r => r.Comments, f => new List<Comment>());

    /// <summary>
    /// Generate a single Review with default values
    /// </summary>
    public static Review Create() => _faker.Generate();

    /// <summary>
    /// Generate multiple Reviews
    /// </summary>
    public static IList<Review> CreateMany(int count = 3) => _faker.Generate(count);

    /// <summary>
    /// Create Reviews for a specific Pull Request
    /// </summary>
    public static IList<Review> CreateManyForPullRequest(long pullRequestId, int count = 3)
    {
        return _faker.Clone()
                     .RuleFor(r => r.PullRequestId, pullRequestId)
                     .RuleFor(r => r.PullRequest, new PullRequest { Id = pullRequestId })
                     .Generate(count);
    }

    /// <summary>
    /// Create an approved Review
    /// </summary>
    public static Review CreateApproved() => _faker.Clone().RuleFor(r => r.State, ReviewState.Approved).Generate();

    /// <summary>
    /// Create a Review requesting changes
    /// </summary>
    public static Review CreateChangesRequested() => _faker.Clone().RuleFor(r => r.State, ReviewState.ChangesRequested).Generate();

    /// <summary>
    /// Create a pending Review
    /// </summary>
    public static Review CreatePending() => _faker.Clone().RuleFor(r => r.State, ReviewState.Pending).Generate();

    /// <summary>
    /// Create a commented Review
    /// </summary>
    public static Review CreateCommented() => _faker.Clone().RuleFor(r => r.State, ReviewState.Commented).Generate();

    /// <summary>
    /// Create a Review from specific reviewer
    /// </summary>
    public static Review FromReviewer(User reviewer) => _faker.Clone().RuleFor(r => r.Author, reviewer).Generate();

    /// <summary>
    /// Create a Review with specific body text
    /// </summary>
    public static Review WithBody(string body) => _faker.Clone().RuleFor(r => r.Body, body).Generate();

    /// <summary>
    /// Create a Review with Comments
    /// </summary>
    public static Review WithComments(int commentCount = 3)
    {
        var review = _faker.Generate();
        var comments = CommentDataBuilder.CreateManyForPullRequest(review.PullRequestId, commentCount)
                                        .Select(c => { c.ReviewId = review.Id; c.Review = review; return c; })
                                        .ToList();
        review.Comments = comments;
        return review;
    }

    /// <summary>
    /// Create Reviews for testing approval workflows
    /// </summary>
    public static IList<Review> CreateApprovalWorkflow(long pullRequestId)
    {
        return new List<Review>
        {
            _faker.Clone()
                  .RuleFor(r => r.PullRequestId, pullRequestId)
                  .RuleFor(r => r.State, ReviewState.ChangesRequested)
                  .RuleFor(r => r.SubmittedAt, DateTimeOffset.Now.AddDays(-3))
                  .Generate(),
            _faker.Clone()
                  .RuleFor(r => r.PullRequestId, pullRequestId)
                  .RuleFor(r => r.State, ReviewState.Commented)
                  .RuleFor(r => r.SubmittedAt, DateTimeOffset.Now.AddDays(-2))
                  .Generate(),
            _faker.Clone()
                  .RuleFor(r => r.PullRequestId, pullRequestId)
                  .RuleFor(r => r.State, ReviewState.Approved)
                  .RuleFor(r => r.SubmittedAt, DateTimeOffset.Now.AddDays(-1))
                  .Generate()
        };
    }

    /// <summary>
    /// Create a batch of Reviews for testing large datasets
    /// </summary>
    public static IList<Review> CreateLargeBatch(int count = 50)
    {
        return _faker.Generate(count);
    }
}