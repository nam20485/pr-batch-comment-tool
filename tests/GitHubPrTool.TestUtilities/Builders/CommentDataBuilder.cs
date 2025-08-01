using Bogus;
using GitHubPrTool.Core.Models;

namespace GitHubPrTool.TestUtilities.Builders;

/// <summary>
/// Test data generator for Comment models using Bogus
/// </summary>
public static class CommentDataBuilder
{
    private static readonly Faker<Comment> _faker = new Faker<Comment>()
        .RuleFor(c => c.Id, f => f.Random.Long(1, 1000000))
        .RuleFor(c => c.Body, f => f.Lorem.Paragraphs())
        .RuleFor(c => c.Type, f => f.PickRandom<CommentType>())
        .RuleFor(c => c.Author, f => UserDataBuilder.CreateMinimal())
        .RuleFor(c => c.PullRequest, f => new PullRequest { Id = f.Random.Long(1, 1000000) })
        .RuleFor(c => c.PullRequestId, (f, c) => c.PullRequest.Id)
        .RuleFor(c => c.HtmlUrl, f => f.Internet.Url())
        .RuleFor(c => c.Path, (f, c) => c.Type == CommentType.Review ? f.System.FilePath() : null)
        .RuleFor(c => c.Line, (f, c) => c.Type == CommentType.Review ? f.Random.Int(1, 1000) : null)
        .RuleFor(c => c.OriginalLine, (f, c) => c.Line.HasValue ? f.Random.Int(1, c.Line.Value) : null)
        .RuleFor(c => c.CommitId, (f, c) => c.Type == CommentType.Review ? f.Random.Hash(40) : null)
        .RuleFor(c => c.OriginalCommitId, (f, c) => c.CommitId != null ? f.Random.Hash(40) : null)
        .RuleFor(c => c.DiffHunk, (f, c) => c.Type == CommentType.Review ? f.Lorem.Lines(3) : null)
        .RuleFor(c => c.Position, (f, c) => c.Type == CommentType.Review ? f.Random.Int(1, 100) : null)
        .RuleFor(c => c.OriginalPosition, (f, c) => c.Position.HasValue ? f.Random.Int(1, c.Position.Value) : null)
        .RuleFor(c => c.IsMultiLine, f => f.Random.Bool(0.3f))
        .RuleFor(c => c.StartLine, (f, c) => c.IsMultiLine && c.Line.HasValue ? f.Random.Int(1, c.Line.Value) : null)
        .RuleFor(c => c.StartSide, (f, c) => c.IsMultiLine ? f.PickRandom("LEFT", "RIGHT") : null)
        .RuleFor(c => c.Side, (f, c) => c.Type == CommentType.Review ? f.PickRandom("LEFT", "RIGHT") : null)
        .RuleFor(c => c.ReviewId, (f, c) => c.Type == CommentType.Review ? f.Random.Long(1, 1000000) : null)
        .RuleFor(c => c.InReplyToId, f => f.Random.Bool(0.2f) ? f.Random.Long(1, 1000000) : null)
        .RuleFor(c => c.CreatedAt, f => f.Date.PastOffset(30))
        .RuleFor(c => c.UpdatedAt, f => f.Date.RecentOffset())
        .RuleFor(c => c.Replies, f => new List<Comment>())
        .RuleFor(c => c.AIInsights, f => new List<AIInsight>());

    /// <summary>
    /// Generate a single Comment with default values
    /// </summary>
    public static Comment Create() => _faker.Generate();

    /// <summary>
    /// Generate multiple Comments
    /// </summary>
    public static IList<Comment> CreateMany(int count = 3) => _faker.Generate(count);

    /// <summary>
    /// Create Comments for a specific Pull Request
    /// </summary>
    public static IList<Comment> CreateManyForPullRequest(long pullRequestId, int count = 3)
    {
        return _faker.RuleFor(c => c.PullRequestId, pullRequestId)
                     .RuleFor(c => c.PullRequest, new PullRequest { Id = pullRequestId })
                     .Generate(count);
    }

    /// <summary>
    /// Create an Issue comment
    /// </summary>
    public static Comment CreateIssueComment() => _faker
        .RuleFor(c => c.Type, CommentType.Issue)
        .RuleFor(c => c.Path, (string?)null)
        .RuleFor(c => c.Line, (int?)null)
        .RuleFor(c => c.ReviewId, (long?)null)
        .Generate();

    /// <summary>
    /// Create a Review comment
    /// </summary>
    public static Comment CreateReviewComment() => _faker
        .RuleFor(c => c.Type, CommentType.Review)
        .RuleFor(c => c.Path, f => f.System.FilePath())
        .RuleFor(c => c.Line, f => f.Random.Int(1, 1000))
        .RuleFor(c => c.ReviewId, f => f.Random.Long(1, 1000000))
        .Generate();

    /// <summary>
    /// Create a Commit comment
    /// </summary>
    public static Comment CreateCommitComment() => _faker
        .RuleFor(c => c.Type, CommentType.Commit)
        .RuleFor(c => c.CommitId, f => f.Random.Hash(40))
        .RuleFor(c => c.Path, f => f.System.FilePath())
        .Generate();

    /// <summary>
    /// Create a Comment with specific body text
    /// </summary>
    public static Comment WithBody(string body) => _faker.RuleFor(c => c.Body, body).Generate();

    /// <summary>
    /// Create a Comment from specific author
    /// </summary>
    public static Comment FromAuthor(User author) => _faker.RuleFor(c => c.Author, author).Generate();

    /// <summary>
    /// Create Comments with replies (threaded conversation)
    /// </summary>
    public static Comment WithReplies(int replyCount = 2)
    {
        var parentComment = _faker.Generate();
        var replies = _faker.RuleFor(c => c.InReplyToId, parentComment.Id)
                            .RuleFor(c => c.InReplyTo, parentComment)
                            .Generate(replyCount);
        
        parentComment.Replies = replies.ToList();
        return parentComment;
    }

    /// <summary>
    /// Create a batch of Comments for testing large datasets
    /// </summary>
    public static IList<Comment> CreateLargeBatch(int count = 100)
    {
        return _faker.Generate(count);
    }

    /// <summary>
    /// Create Comments with specific date range
    /// </summary>
    public static IList<Comment> CreateInDateRange(DateTimeOffset startDate, DateTimeOffset endDate, int count = 10)
    {
        return _faker.RuleFor(c => c.CreatedAt, f => f.Date.BetweenOffset(startDate, endDate))
                     .Generate(count);
    }

    /// <summary>
    /// Create Comments containing specific text (for search testing)
    /// </summary>
    public static IList<Comment> WithBodyContaining(string searchText, int count = 5)
    {
        return _faker.RuleFor(c => c.Body, f => $"{f.Lorem.Sentence()} {searchText} {f.Lorem.Sentence()}")
                     .Generate(count);
    }
}