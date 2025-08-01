using Bogus;
using GitHubPrTool.Core.Models;

namespace GitHubPrTool.TestUtilities.Builders;

/// <summary>
/// Test data generator for PullRequest models using Bogus
/// </summary>
public static class PullRequestDataBuilder
{
    private static readonly Faker<PullRequest> _faker = new Faker<PullRequest>()
        .RuleFor(pr => pr.Id, f => f.Random.Long(1, 1000000))
        .RuleFor(pr => pr.Number, f => f.Random.Int(1, 10000))
        .RuleFor(pr => pr.Title, f => f.Lorem.Sentence())
        .RuleFor(pr => pr.Body, f => f.Lorem.Paragraphs())
        .RuleFor(pr => pr.State, f => f.PickRandom<PullRequestState>())
        .RuleFor(pr => pr.Author, f => UserDataBuilder.CreateMinimal())
        .RuleFor(pr => pr.Repository, f => new Repository { Id = f.Random.Long(1, 1000000) })
        .RuleFor(pr => pr.RepositoryId, (f, pr) => pr.Repository.Id)
        .RuleFor(pr => pr.HeadBranch, f => f.System.FileName())
        .RuleFor(pr => pr.BaseBranch, f => f.PickRandom("main", "master", "develop"))
        .RuleFor(pr => pr.HtmlUrl, f => f.Internet.Url())
        .RuleFor(pr => pr.Mergeable, f => f.Random.Bool())
        .RuleFor(pr => pr.IsDraft, f => f.Random.Bool(0.2f))
        .RuleFor(pr => pr.Additions, f => f.Random.Int(0, 1000))
        .RuleFor(pr => pr.Deletions, f => f.Random.Int(0, 500))
        .RuleFor(pr => pr.ChangedFiles, f => f.Random.Int(1, 50))
        .RuleFor(pr => pr.Commits, f => f.Random.Int(1, 20))
        .RuleFor(pr => pr.CreatedAt, f => f.Date.PastOffset(30))
        .RuleFor(pr => pr.UpdatedAt, f => f.Date.RecentOffset())
        .RuleFor(pr => pr.MergedAt, (f, pr) => pr.State == PullRequestState.Merged ? f.Date.RecentOffset() : null)
        .RuleFor(pr => pr.ClosedAt, (f, pr) => pr.State == PullRequestState.Closed || pr.State == PullRequestState.Merged ? f.Date.RecentOffset() : null)
        .RuleFor(pr => pr.MergedBy, (f, pr) => pr.State == PullRequestState.Merged ? UserDataBuilder.CreateMinimal() : null)
        .RuleFor(pr => pr.Comments, f => new List<Comment>())
        .RuleFor(pr => pr.Reviews, f => new List<Review>());

    /// <summary>
    /// Generate a single PullRequest with default values
    /// </summary>
    public static PullRequest Create() => _faker.Generate();

    /// <summary>
    /// Generate multiple PullRequests
    /// </summary>
    public static IList<PullRequest> CreateMany(int count = 3) => _faker.Generate(count);

    /// <summary>
    /// Create PullRequests for a specific repository
    /// </summary>
    public static IList<PullRequest> CreateManyForRepository(long repositoryId, int count = 3)
    {
        return _faker.Clone()
                     .RuleFor(pr => pr.RepositoryId, repositoryId)
                     .RuleFor(pr => pr.Repository, new Repository { Id = repositoryId })
                     .Generate(count);
    }

    /// <summary>
    /// Create an open PullRequest
    /// </summary>
    public static PullRequest CreateOpen() => _faker.Clone().RuleFor(pr => pr.State, PullRequestState.Open).Generate();

    /// <summary>
    /// Create a closed PullRequest
    /// </summary>
    public static PullRequest CreateClosed() => _faker.Clone().RuleFor(pr => pr.State, PullRequestState.Closed).Generate();

    /// <summary>
    /// Create a merged PullRequest
    /// </summary>
    public static PullRequest CreateMerged() => _faker.Clone()
        .RuleFor(pr => pr.State, PullRequestState.Merged)
        .RuleFor(pr => pr.MergedAt, f => f.Date.RecentOffset())
        .RuleFor(pr => pr.MergedBy, f => UserDataBuilder.CreateMinimal())
        .Generate();

    /// <summary>
    /// Create a draft PullRequest
    /// </summary>
    public static PullRequest CreateDraft() => _faker.Clone().RuleFor(pr => pr.IsDraft, true).Generate();

    /// <summary>
    /// Create a PullRequest with specific title
    /// </summary>
    public static PullRequest WithTitle(string title) => _faker.Clone().RuleFor(pr => pr.Title, title).Generate();

    /// <summary>
    /// Create a PullRequest with specific author
    /// </summary>
    public static PullRequest WithAuthor(User author) => _faker.Clone().RuleFor(pr => pr.Author, author).Generate();

    /// <summary>
    /// Create a PullRequest with comments
    /// </summary>
    public static PullRequest WithComments(int count = 3)
    {
        var pr = _faker.Generate();
        pr.Comments = CommentDataBuilder.CreateManyForPullRequest(pr.Id, count).ToList();
        return pr;
    }

    /// <summary>
    /// Create a PullRequest with reviews
    /// </summary>
    public static PullRequest WithReviews(int count = 2)
    {
        var pr = _faker.Generate();
        pr.Reviews = ReviewDataBuilder.CreateManyForPullRequest(pr.Id, count).ToList();
        return pr;
    }
}