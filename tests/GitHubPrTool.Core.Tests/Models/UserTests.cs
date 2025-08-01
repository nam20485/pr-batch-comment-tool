using FluentAssertions;
using GitHubPrTool.Core.Models;
using GitHubPrTool.TestUtilities.Builders;

namespace GitHubPrTool.Core.Tests.Models;

/// <summary>
/// Tests for the User model
/// </summary>
public class UserTests
{
    [Fact]
    public void User_DefaultConstructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var user = new User();

        // Assert
        user.Id.Should().Be(0);
        user.Login.Should().Be(string.Empty);
        user.Name.Should().BeNull();
        user.Email.Should().BeNull();
        user.AvatarUrl.Should().BeNull();
        user.HtmlUrl.Should().BeNull();
        user.Bio.Should().BeNull();
        user.CreatedAt.Should().BeNull();
        user.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void User_WithProperties_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var expectedId = 12345L;
        var expectedLogin = "testuser";
        var expectedName = "Test User";
        var expectedEmail = "test@example.com";
        var expectedAvatarUrl = "https://example.com/avatar.png";
        var expectedHtmlUrl = "https://github.com/testuser";
        var expectedBio = "A test user";
        var expectedCreatedAt = DateTimeOffset.Now.AddYears(-2);
        var expectedUpdatedAt = DateTimeOffset.Now;

        // Act
        var user = new User
        {
            Id = expectedId,
            Login = expectedLogin,
            Name = expectedName,
            Email = expectedEmail,
            AvatarUrl = expectedAvatarUrl,
            HtmlUrl = expectedHtmlUrl,
            Bio = expectedBio,
            CreatedAt = expectedCreatedAt,
            UpdatedAt = expectedUpdatedAt
        };

        // Assert
        user.Id.Should().Be(expectedId);
        user.Login.Should().Be(expectedLogin);
        user.Name.Should().Be(expectedName);
        user.Email.Should().Be(expectedEmail);
        user.AvatarUrl.Should().Be(expectedAvatarUrl);
        user.HtmlUrl.Should().Be(expectedHtmlUrl);
        user.Bio.Should().Be(expectedBio);
        user.CreatedAt.Should().Be(expectedCreatedAt);
        user.UpdatedAt.Should().Be(expectedUpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("valid-username")]
    [InlineData("user123")]
    [InlineData("User-With-Hyphens")]
    public void User_Login_ShouldAcceptVariousFormats(string login)
    {
        // Act
        var user = new User { Login = login };

        // Assert
        user.Login.Should().Be(login);
    }

    [Fact]
    public void User_CreatedWithBuilder_ShouldHaveValidData()
    {
        // Act
        var user = UserDataBuilder.Create();

        // Assert
        user.Id.Should().BeGreaterThan(0);
        user.Login.Should().NotBeNullOrEmpty();
        user.Name.Should().NotBeNullOrEmpty();
        user.Email.Should().NotBeNullOrEmpty();
        user.AvatarUrl.Should().NotBeNullOrEmpty();
        user.HtmlUrl.Should().NotBeNullOrEmpty();
        user.CreatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void User_WithSpecificLogin_ShouldSetLogin()
    {
        // Arrange
        var expectedLogin = "specific-user";

        // Act
        var user = UserDataBuilder.WithLogin(expectedLogin);

        // Assert
        user.Login.Should().Be(expectedLogin);
        user.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public void User_WithSpecificId_ShouldSetId()
    {
        // Arrange
        var expectedId = 999L;

        // Act
        var user = UserDataBuilder.WithId(expectedId);

        // Assert
        user.Id.Should().Be(expectedId);
        user.Login.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void User_MinimalCreation_ShouldHaveRequiredProperties()
    {
        // Act
        var user = UserDataBuilder.CreateMinimal();

        // Assert
        user.Id.Should().BeGreaterThan(0);
        user.Login.Should().NotBeNullOrEmpty();
        // Optional properties may be null for minimal creation
    }

    [Fact]
    public void UserCollection_CreateMany_ShouldGenerateUniqueUsers()
    {
        // Act
        var users = UserDataBuilder.CreateMany(5);

        // Assert
        users.Should().HaveCount(5);
        users.Select(u => u.Id).Should().OnlyHaveUniqueItems();
        users.Select(u => u.Login).Should().OnlyHaveUniqueItems();
        users.Should().AllSatisfy(u => u.Id.Should().BeGreaterThan(0));
        users.Should().AllSatisfy(u => u.Login.Should().NotBeNullOrEmpty());
    }
}