using FluentAssertions;
using GitHubPrTool.Core.Models;
using GitHubPrTool.TestUtilities.Builders;

namespace GitHubPrTool.Core.Tests.Models;

/// <summary>
/// Tests for the Repository model
/// </summary>
public class RepositoryTests
{
    [Fact]
    public void Repository_DefaultConstructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var repository = new Repository();

        // Assert
        repository.Id.Should().Be(0);
        repository.Name.Should().Be(string.Empty);
        repository.FullName.Should().Be(string.Empty);
        repository.Description.Should().BeNull();
        repository.HtmlUrl.Should().BeNull();
        repository.CloneUrl.Should().BeNull();
        repository.Owner.Should().NotBeNull();
        repository.Private.Should().BeFalse();
        repository.DefaultBranch.Should().Be("main");
        repository.Language.Should().BeNull();
        repository.StargazersCount.Should().Be(0);
        repository.ForksCount.Should().Be(0);
        repository.OpenIssuesCount.Should().Be(0);
        repository.CreatedAt.Should().Be(default);
        repository.UpdatedAt.Should().Be(default);
        repository.PushedAt.Should().BeNull();
        repository.PullRequests.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Repository_WithAllProperties_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var expectedId = 12345L;
        var expectedName = "test-repo";
        var expectedFullName = "owner/test-repo";
        var expectedDescription = "A test repository";
        var expectedHtmlUrl = "https://github.com/owner/test-repo";
        var expectedCloneUrl = "https://github.com/owner/test-repo.git";
        var expectedOwner = UserDataBuilder.Create();
        var expectedPrivate = true;
        var expectedDefaultBranch = "develop";
        var expectedLanguage = "C#";
        var expectedStargazersCount = 100;
        var expectedForksCount = 25;
        var expectedOpenIssuesCount = 5;
        var expectedCreatedAt = DateTimeOffset.Now.AddYears(-2);
        var expectedUpdatedAt = DateTimeOffset.Now;
        var expectedPushedAt = DateTimeOffset.Now.AddDays(-1);

        // Act
        var repository = new Repository
        {
            Id = expectedId,
            Name = expectedName,
            FullName = expectedFullName,
            Description = expectedDescription,
            HtmlUrl = expectedHtmlUrl,
            CloneUrl = expectedCloneUrl,
            Owner = expectedOwner,
            Private = expectedPrivate,
            DefaultBranch = expectedDefaultBranch,
            Language = expectedLanguage,
            StargazersCount = expectedStargazersCount,
            ForksCount = expectedForksCount,
            OpenIssuesCount = expectedOpenIssuesCount,
            CreatedAt = expectedCreatedAt,
            UpdatedAt = expectedUpdatedAt,
            PushedAt = expectedPushedAt
        };

        // Assert
        repository.Id.Should().Be(expectedId);
        repository.Name.Should().Be(expectedName);
        repository.FullName.Should().Be(expectedFullName);
        repository.Description.Should().Be(expectedDescription);
        repository.HtmlUrl.Should().Be(expectedHtmlUrl);
        repository.CloneUrl.Should().Be(expectedCloneUrl);
        repository.Owner.Should().Be(expectedOwner);
        repository.Private.Should().Be(expectedPrivate);
        repository.DefaultBranch.Should().Be(expectedDefaultBranch);
        repository.Language.Should().Be(expectedLanguage);
        repository.StargazersCount.Should().Be(expectedStargazersCount);
        repository.ForksCount.Should().Be(expectedForksCount);
        repository.OpenIssuesCount.Should().Be(expectedOpenIssuesCount);
        repository.CreatedAt.Should().Be(expectedCreatedAt);
        repository.UpdatedAt.Should().Be(expectedUpdatedAt);
        repository.PushedAt.Should().Be(expectedPushedAt);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Repository_Private_ShouldAcceptBooleanValues(bool isPrivate)
    {
        // Act
        var repository = new Repository { Private = isPrivate };

        // Assert
        repository.Private.Should().Be(isPrivate);
    }

    [Theory]
    [InlineData("main")]
    [InlineData("master")]
    [InlineData("develop")]
    [InlineData("custom-branch")]
    public void Repository_DefaultBranch_ShouldAcceptVariousBranchNames(string branchName)
    {
        // Act
        var repository = new Repository { DefaultBranch = branchName };

        // Assert
        repository.DefaultBranch.Should().Be(branchName);
    }

    [Fact]
    public void Repository_CreatedWithBuilder_ShouldHaveValidData()
    {
        // Act
        var repository = RepositoryDataBuilder.Create();

        // Assert
        repository.Id.Should().BeGreaterThan(0);
        repository.Name.Should().NotBeNullOrEmpty();
        repository.FullName.Should().NotBeNullOrEmpty();
        repository.Owner.Should().NotBeNull();
        repository.Owner.Id.Should().BeGreaterThan(0);
        repository.DefaultBranch.Should().NotBeNullOrEmpty();
        repository.CreatedAt.Should().NotBe(default);
        repository.UpdatedAt.Should().NotBe(default);
    }

    [Fact]
    public void Repository_WithSpecificName_ShouldSetName()
    {
        // Arrange
        var expectedName = "specific-repo-name";

        // Act
        var repository = RepositoryDataBuilder.WithName(expectedName);

        // Assert
        repository.Name.Should().Be(expectedName);
    }

    [Fact]
    public void Repository_WithSpecificOwner_ShouldSetOwner()
    {
        // Arrange
        var expectedOwner = UserDataBuilder.WithLogin("repo-owner");

        // Act
        var repository = RepositoryDataBuilder.WithOwner(expectedOwner);

        // Assert
        repository.Owner.Should().Be(expectedOwner);
        repository.Owner.Login.Should().Be("repo-owner");
    }

    [Fact]
    public void Repository_CreatePublic_ShouldNotBePrivate()
    {
        // Act
        var repository = RepositoryDataBuilder.CreatePublic();

        // Assert
        repository.Private.Should().BeFalse();
    }

    [Fact]
    public void Repository_CreatePrivate_ShouldBePrivate()
    {
        // Act
        var repository = RepositoryDataBuilder.CreatePrivate();

        // Assert
        repository.Private.Should().BeTrue();
    }

    [Fact]
    public void Repository_WithSpecificLanguage_ShouldSetLanguage()
    {
        // Arrange
        var expectedLanguage = "TypeScript";

        // Act
        var repository = RepositoryDataBuilder.WithLanguage(expectedLanguage);

        // Assert
        repository.Language.Should().Be(expectedLanguage);
    }

    [Fact]
    public void Repository_WithPullRequests_ShouldHavePullRequests()
    {
        // Act
        var repository = RepositoryDataBuilder.WithPullRequests(4);

        // Assert
        repository.PullRequests.Should().HaveCount(4);
        repository.PullRequests.Should().AllSatisfy(pr =>
        {
            pr.RepositoryId.Should().Be(repository.Id);
        });
    }

    [Fact]
    public void RepositoryCollection_CreateMany_ShouldGenerateUniqueRepositories()
    {
        // Act
        var repositories = RepositoryDataBuilder.CreateMany(5);

        // Assert
        repositories.Should().HaveCount(5);
        repositories.Select(r => r.Id).Should().OnlyHaveUniqueItems();
        repositories.Select(r => r.Name).Should().OnlyHaveUniqueItems();
        repositories.Should().AllSatisfy(r => r.Id.Should().BeGreaterThan(0));
        repositories.Should().AllSatisfy(r => r.Name.Should().NotBeNullOrEmpty());
        repositories.Should().AllSatisfy(r => r.Owner.Should().NotBeNull());
    }

    [Fact]
    public void Repository_Statistics_ShouldHaveNonNegativeValues()
    {
        // Act
        var repository = RepositoryDataBuilder.Create();

        // Assert
        repository.StargazersCount.Should().BeGreaterOrEqualTo(0);
        repository.ForksCount.Should().BeGreaterOrEqualTo(0);
        repository.OpenIssuesCount.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void Repository_FullName_ShouldFollowOwnerNamePattern()
    {
        // Arrange
        var owner = UserDataBuilder.WithLogin("test-owner");
        var repositoryName = "test-repo";

        // Act
        var repository = new Repository
        {
            Name = repositoryName,
            Owner = owner,
            FullName = $"{owner.Login}/{repositoryName}"
        };

        // Assert
        repository.FullName.Should().Be("test-owner/test-repo");
        repository.FullName.Should().StartWith(owner.Login);
        repository.FullName.Should().EndWith(repositoryName);
        repository.FullName.Should().Contain("/");
    }

    [Theory]
    [InlineData("C#")]
    [InlineData("JavaScript")]
    [InlineData("Python")]
    [InlineData("Java")]
    [InlineData("Go")]
    [InlineData("Rust")]
    [InlineData("TypeScript")]
    public void Repository_Language_ShouldAcceptVariousLanguages(string language)
    {
        // Act
        var repository = new Repository { Language = language };

        // Assert
        repository.Language.Should().Be(language);
    }

    [Fact]
    public void Repository_Timestamps_ShouldHaveLogicalOrder()
    {
        // Arrange
        var now = DateTimeOffset.Now;
        var createdAt = now.AddYears(-1);
        var updatedAt = now.AddDays(-1);
        var pushedAt = now;

        // Act
        var repository = new Repository
        {
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            PushedAt = pushedAt
        };

        // Assert
        repository.CreatedAt.Should().BeBefore(repository.UpdatedAt);
        repository.UpdatedAt.Should().BeOnOrBefore(repository.PushedAt.Value);
    }
}