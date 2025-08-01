using Bogus;
using GitHubPrTool.Core.Models;

namespace GitHubPrTool.TestUtilities.Builders;

/// <summary>
/// Test data generator for Repository models using Bogus
/// </summary>
public static class RepositoryDataBuilder
{
    private static readonly Faker<Repository> _faker = new Faker<Repository>()
        .RuleFor(r => r.Id, f => f.Random.Long(1, 1000000))
        .RuleFor(r => r.Name, f => f.System.FileName().Replace(".", ""))
        .RuleFor(r => r.FullName, (f, r) => $"{f.Internet.UserName()}/{r.Name}")
        .RuleFor(r => r.Description, f => f.Lorem.Sentence())
        .RuleFor(r => r.Private, f => f.Random.Bool())
        .RuleFor(r => r.Owner, f => UserDataBuilder.CreateMinimal())
        .RuleFor(r => r.HtmlUrl, f => f.Internet.Url())
        .RuleFor(r => r.CloneUrl, f => f.Internet.Url() + ".git")
        .RuleFor(r => r.DefaultBranch, f => f.PickRandom("main", "master", "develop"))
        .RuleFor(r => r.Language, f => f.PickRandom("C#", "JavaScript", "Python", "Java", "Go"))
        .RuleFor(r => r.ForksCount, f => f.Random.Int(0, 1000))
        .RuleFor(r => r.StargazersCount, f => f.Random.Int(0, 5000))
        .RuleFor(r => r.OpenIssuesCount, f => f.Random.Int(0, 200))
        .RuleFor(r => r.CreatedAt, f => f.Date.PastOffset(5))
        .RuleFor(r => r.UpdatedAt, f => f.Date.RecentOffset())
        .RuleFor(r => r.PushedAt, f => f.Date.RecentOffset())
        .RuleFor(r => r.PullRequests, f => new List<PullRequest>());

    /// <summary>
    /// Generate a single Repository with default values
    /// </summary>
    public static Repository Create() => _faker.Generate();

    /// <summary>
    /// Generate multiple Repositories
    /// </summary>
    public static IList<Repository> CreateMany(int count = 3) => _faker.Generate(count);

    /// <summary>
    /// Create a Repository with specific name
    /// </summary>
    public static Repository WithName(string name) => _faker.RuleFor(r => r.Name, name).Generate();

    /// <summary>
    /// Create a Repository with specific owner
    /// </summary>
    public static Repository WithOwner(User owner) => _faker.RuleFor(r => r.Owner, owner).Generate();

    /// <summary>
    /// Create a public Repository
    /// </summary>
    public static Repository CreatePublic() => _faker.RuleFor(r => r.Private, false).Generate();

    /// <summary>
    /// Create a private Repository
    /// </summary>
    public static Repository CreatePrivate() => _faker.RuleFor(r => r.Private, true).Generate();

    /// <summary>
    /// Create a Repository with specific language
    /// </summary>
    public static Repository WithLanguage(string language) => _faker.RuleFor(r => r.Language, language).Generate();

    /// <summary>
    /// Create a Repository with Pull Requests
    /// </summary>
    public static Repository WithPullRequests(int count = 3) 
    {
        var repo = _faker.Generate();
        var pullRequests = PullRequestDataBuilder.CreateManyForRepository(repo.Id, count);
        repo.PullRequests = pullRequests.ToList();
        return repo;
    }
}