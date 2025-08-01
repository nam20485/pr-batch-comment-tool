using FluentAssertions;
using GitHubPrTool.Core.Models;
using GitHubPrTool.TestUtilities.Builders;

namespace GitHubPrTool.Core.Tests.Models;

/// <summary>
/// Tests for the Comment model and CommentType enum
/// </summary>
public class CommentTests
{
    [Fact]
    public void Comment_DefaultConstructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var comment = new Comment();

        // Assert
        comment.Id.Should().Be(0);
        comment.Body.Should().Be(string.Empty);
        comment.Type.Should().Be(CommentType.Issue);
        comment.Author.Should().NotBeNull();
        comment.PullRequest.Should().NotBeNull();
        comment.PullRequestId.Should().Be(0);
        comment.HtmlUrl.Should().BeNull();
        comment.Path.Should().BeNull();
        comment.Line.Should().BeNull();
        comment.ReviewId.Should().BeNull();
        comment.InReplyToId.Should().BeNull();
        comment.CreatedAt.Should().Be(default);
        comment.UpdatedAt.Should().Be(default);
        comment.Replies.Should().NotBeNull().And.BeEmpty();
        comment.AIInsights.Should().NotBeNull().And.BeEmpty();
    }

    [Theory]
    [InlineData(CommentType.Issue)]
    [InlineData(CommentType.Review)]
    [InlineData(CommentType.Commit)]
    public void Comment_Type_ShouldAcceptAllValidTypes(CommentType type)
    {
        // Act
        var comment = new Comment { Type = type };

        // Assert
        comment.Type.Should().Be(type);
    }

    [Fact]
    public void Comment_WithAllProperties_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var expectedId = 12345L;
        var expectedBody = "This is a test comment";
        var expectedType = CommentType.Review;
        var expectedAuthor = UserDataBuilder.Create();
        var expectedPullRequestId = 98765L;
        var expectedHtmlUrl = "https://github.com/owner/repo/pull/1#issuecomment-12345";
        var expectedPath = "src/test.cs";
        var expectedLine = 42;
        var expectedCreatedAt = DateTimeOffset.Now.AddDays(-1);
        var expectedUpdatedAt = DateTimeOffset.Now;

        // Act
        var comment = new Comment
        {
            Id = expectedId,
            Body = expectedBody,
            Type = expectedType,
            Author = expectedAuthor,
            PullRequestId = expectedPullRequestId,
            HtmlUrl = expectedHtmlUrl,
            Path = expectedPath,
            Line = expectedLine,
            CreatedAt = expectedCreatedAt,
            UpdatedAt = expectedUpdatedAt
        };

        // Assert
        comment.Id.Should().Be(expectedId);
        comment.Body.Should().Be(expectedBody);
        comment.Type.Should().Be(expectedType);
        comment.Author.Should().Be(expectedAuthor);
        comment.PullRequestId.Should().Be(expectedPullRequestId);
        comment.HtmlUrl.Should().Be(expectedHtmlUrl);
        comment.Path.Should().Be(expectedPath);
        comment.Line.Should().Be(expectedLine);
        comment.CreatedAt.Should().Be(expectedCreatedAt);
        comment.UpdatedAt.Should().Be(expectedUpdatedAt);
    }

    [Fact]
    public void Comment_CreatedWithBuilder_ShouldHaveValidData()
    {
        // Act
        var comment = CommentDataBuilder.Create();

        // Assert
        comment.Id.Should().BeGreaterThan(0);
        comment.Body.Should().NotBeNullOrEmpty();
        comment.Author.Should().NotBeNull();
        comment.Author.Id.Should().BeGreaterThan(0);
        comment.PullRequestId.Should().BeGreaterThan(0);
        comment.CreatedAt.Should().NotBe(default);
        comment.UpdatedAt.Should().NotBe(default);
    }

    [Fact]
    public void Comment_IssueType_ShouldNotHaveReviewSpecificProperties()
    {
        // Act
        var comment = CommentDataBuilder.CreateIssueComment();

        // Assert
        comment.Type.Should().Be(CommentType.Issue);
        comment.Path.Should().BeNull();
        comment.Line.Should().BeNull();
        comment.ReviewId.Should().BeNull();
    }

    [Fact]
    public void Comment_ReviewType_ShouldHaveReviewSpecificProperties()
    {
        // Act
        var comment = CommentDataBuilder.CreateReviewComment();

        // Assert
        comment.Type.Should().Be(CommentType.Review);
        comment.Path.Should().NotBeNull();
        comment.Line.Should().BeGreaterThan(0);
        comment.ReviewId.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Comment_CommitType_ShouldHaveCommitSpecificProperties()
    {
        // Act
        var comment = CommentDataBuilder.CreateCommitComment();

        // Assert
        comment.Type.Should().Be(CommentType.Commit);
        comment.CommitId.Should().NotBeNull();
        comment.Path.Should().NotBeNull();
    }

    [Fact]
    public void Comment_WithSpecificBody_ShouldSetBody()
    {
        // Arrange
        var expectedBody = "This is a specific comment body for testing";

        // Act
        var comment = CommentDataBuilder.WithBody(expectedBody);

        // Assert
        comment.Body.Should().Be(expectedBody);
    }

    [Fact]
    public void Comment_FromSpecificAuthor_ShouldSetAuthor()
    {
        // Arrange
        var expectedAuthor = UserDataBuilder.WithLogin("specific-author");

        // Act
        var comment = CommentDataBuilder.FromAuthor(expectedAuthor);

        // Assert
        comment.Author.Should().Be(expectedAuthor);
        comment.Author.Login.Should().Be("specific-author");
    }

    [Fact]
    public void Comment_WithReplies_ShouldCreateThreadedConversation()
    {
        // Act
        var parentComment = CommentDataBuilder.WithReplies(3);

        // Assert
        parentComment.Replies.Should().HaveCount(3);
        parentComment.Replies.Should().AllSatisfy(reply =>
        {
            reply.InReplyToId.Should().Be(parentComment.Id);
            reply.InReplyTo.Should().Be(parentComment);
        });
    }

    [Fact]
    public void Comment_MultiLineReview_ShouldSetMultiLineProperties()
    {
        // Arrange
        var comment = CommentDataBuilder.CreateReviewComment();
        comment.IsMultiLine = true;
        comment.StartLine = 10;
        comment.Line = 15;

        // Assert
        comment.IsMultiLine.Should().BeTrue();
        comment.StartLine.Should().Be(10);
        comment.Line.Should().Be(15);
        comment.StartLine.Should().BeLessThan(comment.Line.Value);
    }

    [Fact]
    public void CommentCollection_CreateMany_ShouldGenerateUniqueComments()
    {
        // Act
        var comments = CommentDataBuilder.CreateMany(10);

        // Assert
        comments.Should().HaveCount(10);
        comments.Select(c => c.Id).Should().OnlyHaveUniqueItems();
        comments.Should().AllSatisfy(c => c.Id.Should().BeGreaterThan(0));
        comments.Should().AllSatisfy(c => c.Body.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public void CommentCollection_CreateLargeBatch_ShouldHandleHighVolume()
    {
        // Act
        var comments = CommentDataBuilder.CreateLargeBatch(100);

        // Assert
        comments.Should().HaveCount(100);
        comments.Select(c => c.Id).Should().OnlyHaveUniqueItems();
        comments.Should().AllSatisfy(c => c.Author.Should().NotBeNull());
    }

    [Fact]
    public void CommentCollection_WithSpecificDateRange_ShouldRespectDateConstraints()
    {
        // Arrange
        var startDate = DateTimeOffset.Now.AddDays(-30);
        var endDate = DateTimeOffset.Now.AddDays(-1);

        // Act
        var comments = CommentDataBuilder.CreateInDateRange(startDate, endDate, 5);

        // Assert
        comments.Should().HaveCount(5);
        comments.Should().AllSatisfy(c =>
        {
            c.CreatedAt.Should().BeOnOrAfter(startDate);
            c.CreatedAt.Should().BeOnOrBefore(endDate);
        });
    }

    [Fact]
    public void CommentCollection_WithBodyContaining_ShouldIncludeSearchText()
    {
        // Arrange
        var searchText = "UNIQUE_SEARCH_TERM";

        // Act
        var comments = CommentDataBuilder.WithBodyContaining(searchText, 3);

        // Assert
        comments.Should().HaveCount(3);
        comments.Should().AllSatisfy(c => c.Body.Should().Contain(searchText));
    }
}