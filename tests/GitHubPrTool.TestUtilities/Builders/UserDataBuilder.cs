using Bogus;
using GitHubPrTool.Core.Models;

namespace GitHubPrTool.TestUtilities.Builders;

/// <summary>
/// Test data generator for User models using Bogus
/// </summary>
public static class UserDataBuilder
{
    private static readonly Faker<User> _faker = new Faker<User>()
        .RuleFor(u => u.Id, f => f.Random.Long(1, 1000000))
        .RuleFor(u => u.Login, f => f.Internet.UserName())
        .RuleFor(u => u.Name, f => f.Person.FullName)
        .RuleFor(u => u.Email, f => f.Internet.Email())
        .RuleFor(u => u.AvatarUrl, f => f.Internet.Avatar())
        .RuleFor(u => u.Bio, f => f.Lorem.Paragraph())
        .RuleFor(u => u.HtmlUrl, f => f.Internet.Url())
        .RuleFor(u => u.CreatedAt, f => f.Date.PastOffset(5))
        .RuleFor(u => u.UpdatedAt, f => f.Date.RecentOffset());

    /// <summary>
    /// Generate a single User with default values
    /// </summary>
    public static User Create() => _faker.Generate();

    /// <summary>
    /// Generate multiple Users
    /// </summary>
    public static IList<User> CreateMany(int count = 3) => _faker.Generate(count);

    /// <summary>
    /// Create a User with specific login
    /// </summary>
    public static User WithLogin(string login) => _faker.Clone().RuleFor(u => u.Login, login).Generate();

    /// <summary>
    /// Create a User with specific ID
    /// </summary>
    public static User WithId(long id) => _faker.Clone().RuleFor(u => u.Id, id).Generate();

    /// <summary>
    /// Create a minimal User (only ID and Login)
    /// </summary>
    public static User CreateMinimal()
    {
        var faker = new Faker();
        return new User
        {
            Id = faker.Random.Long(1, 1000000),
            Login = faker.Internet.UserName()
        };
    }
}