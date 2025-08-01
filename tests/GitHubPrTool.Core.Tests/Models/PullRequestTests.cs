using FluentAssertions;
using GitHubPrTool.Core.Models;
using GitHubPrTool.TestUtilities.Builders;

namespace GitHubPrTool.Core.Tests.Models;

/// <summary>
/// Tests for the PullRequest model and PullRequestState enum
/// </summary>
public class PullRequestTests
{
    [Fact]
    public void PullRequest_DefaultConstructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var pullRequest = new PullRequest();

        // Assert
        pullRequest.Id.Should().Be(0);
        pullRequest.Number.Should().Be(0);
        pullRequest.Title.Should().Be(string.Empty);
        pullRequest.Body.Should().BeNull();
        pullRequest.State.Should().Be(PullRequestState.Open);
        pullRequest.HtmlUrl.Should().BeNull();
        pullRequest.Author.Should().NotBeNull();
        pullRequest.Repository.Should().NotBeNull();
        pullRequest.RepositoryId.Should().Be(0);
        pullRequest.BaseBranch.Should().Be(string.Empty);
        pullRequest.HeadBranch.Should().Be(string.Empty);
        pullRequest.IsDraft.Should().BeFalse();
        pullRequest.Mergeable.Should().BeNull();
        pullRequest.Commits.Should().Be(0);
        pullRequest.Additions.Should().Be(0);
        pullRequest.Deletions.Should().Be(0);
        pullRequest.ChangedFiles.Should().Be(0);
        pullRequest.CreatedAt.Should().Be(default);
        pullRequest.UpdatedAt.Should().Be(default);
        pullRequest.ClosedAt.Should().BeNull();
        pullRequest.MergedAt.Should().BeNull();
        pullRequest.MergedBy.Should().BeNull();
        pullRequest.Reviews.Should().NotBeNull().And.BeEmpty();
        pullRequest.Comments.Should().NotBeNull().And.BeEmpty();
    }

    [Theory]
    [InlineData(PullRequestState.Open)]
    [InlineData(PullRequestState.Closed)]
    [InlineData(PullRequestState.Merged)]
    public void PullRequest_State_ShouldAcceptAllValidStates(PullRequestState state)
    {
        // Act
        var pullRequest = new PullRequest { State = state };

        // Assert
        pullRequest.State.Should().Be(state);
    }

    [Fact]
    public void PullRequest_WithAllProperties_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var expectedId = 12345L;
        var expectedNumber = 42;
        var expectedTitle = "Add new feature";
        var expectedBody = "This PR adds a new feature to the application";
        var expectedState = PullRequestState.Open;
        var expectedAuthor = UserDataBuilder.Create();
        var expectedRepositoryId = 98765L;
        var expectedBaseBranch = "main";
        var expectedHeadBranch = "feature/new-feature";
        var expectedIsDraft = true;
        var expectedMergeable = true;
        var expectedCommits = 5;
        var expectedAdditions = 150;
        var expectedDeletions = 25;
        var expectedChangedFiles = 8;
        var expectedCreatedAt = DateTimeOffset.Now.AddDays(-3);
        var expectedUpdatedAt = DateTimeOffset.Now;

        // Act
        var pullRequest = new PullRequest
        {
            Id = expectedId,
            Number = expectedNumber,
            Title = expectedTitle,
            Body = expectedBody,
            State = expectedState,
            Author = expectedAuthor,
            RepositoryId = expectedRepositoryId,
            BaseBranch = expectedBaseBranch,
            HeadBranch = expectedHeadBranch,
            IsDraft = expectedIsDraft,
            Mergeable = expectedMergeable,
            Commits = expectedCommits,
            Additions = expectedAdditions,
            Deletions = expectedDeletions,
            ChangedFiles = expectedChangedFiles,
            CreatedAt = expectedCreatedAt,
            UpdatedAt = expectedUpdatedAt
        };

        // Assert
        pullRequest.Id.Should().Be(expectedId);
        pullRequest.Number.Should().Be(expectedNumber);
        pullRequest.Title.Should().Be(expectedTitle);
        pullRequest.Body.Should().Be(expectedBody);
        pullRequest.State.Should().Be(expectedState);
        pullRequest.Author.Should().Be(expectedAuthor);
        pullRequest.RepositoryId.Should().Be(expectedRepositoryId);
        pullRequest.BaseBranch.Should().Be(expectedBaseBranch);
        pullRequest.HeadBranch.Should().Be(expectedHeadBranch);
        pullRequest.IsDraft.Should().Be(expectedIsDraft);
        pullRequest.Mergeable.Should().Be(expectedMergeable);
        pullRequest.Commits.Should().Be(expectedCommits);
        pullRequest.Additions.Should().Be(expectedAdditions);
        pullRequest.Deletions.Should().Be(expectedDeletions);
        pullRequest.ChangedFiles.Should().Be(expectedChangedFiles);
        pullRequest.CreatedAt.Should().Be(expectedCreatedAt);
        pullRequest.UpdatedAt.Should().Be(expectedUpdatedAt);
    }

    [Fact]
    public void PullRequest_CreatedWithBuilder_ShouldHaveValidData()
    {
        // Act
        var pullRequest = PullRequestDataBuilder.Create();

        // Assert
        pullRequest.Id.Should().BeGreaterThan(0);
        pullRequest.Number.Should().BeGreaterThan(0);
        pullRequest.Title.Should().NotBeNullOrEmpty();
        pullRequest.Author.Should().NotBeNull();
        pullRequest.Author.Id.Should().BeGreaterThan(0);
        pullRequest.RepositoryId.Should().BeGreaterThan(0);
        pullRequest.BaseBranch.Should().NotBeNullOrEmpty();
        pullRequest.HeadBranch.Should().NotBeNullOrEmpty();
        pullRequest.CreatedAt.Should().NotBe(default);
        pullRequest.UpdatedAt.Should().NotBe(default);
    }

    [Fact]
    public void PullRequest_CreateOpen_ShouldBeInOpenState()
    {
        // Act
        var pullRequest = PullRequestDataBuilder.CreateOpen();

        // Assert
        pullRequest.State.Should().Be(PullRequestState.Open);
        pullRequest.ClosedAt.Should().BeNull();
        pullRequest.MergedAt.Should().BeNull();
        pullRequest.MergedBy.Should().BeNull();
    }

    [Fact]
    public void PullRequest_CreateClosed_ShouldBeInClosedState()
    {
        // Act
        var pullRequest = PullRequestDataBuilder.CreateClosed();

        // Assert
        pullRequest.State.Should().Be(PullRequestState.Closed);
    }

    [Fact]
    public void PullRequest_CreateMerged_ShouldBeInMergedStateWithMergeData()
    {
        // Act
        var pullRequest = PullRequestDataBuilder.CreateMerged();

        // Assert
        pullRequest.State.Should().Be(PullRequestState.Merged);
        pullRequest.MergedAt.Should().NotBeNull();
        pullRequest.MergedBy.Should().NotBeNull();
        pullRequest.MergedBy!.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public void PullRequest_CreateDraft_ShouldBeDraft()
    {
        // Act
        var pullRequest = PullRequestDataBuilder.CreateDraft();

        // Assert
        pullRequest.IsDraft.Should().BeTrue();
    }

    [Fact]
    public void PullRequest_WithSpecificTitle_ShouldSetTitle()
    {
        // Arrange
        var expectedTitle = "Specific PR Title";

        // Act
        var pullRequest = PullRequestDataBuilder.WithTitle(expectedTitle);

        // Assert
        pullRequest.Title.Should().Be(expectedTitle);
    }

    [Fact]
    public void PullRequest_WithSpecificAuthor_ShouldSetAuthor()
    {
        // Arrange
        var expectedAuthor = UserDataBuilder.WithLogin("pr-author");

        // Act
        var pullRequest = PullRequestDataBuilder.WithAuthor(expectedAuthor);

        // Assert
        pullRequest.Author.Should().Be(expectedAuthor);
        pullRequest.Author.Login.Should().Be("pr-author");
    }

    [Fact]
    public void PullRequest_WithComments_ShouldHaveComments()
    {
        // Act
        var pullRequest = PullRequestDataBuilder.WithComments(5);

        // Assert
        pullRequest.Comments.Should().HaveCount(5);
        pullRequest.Comments.Should().AllSatisfy(comment =>
        {
            comment.PullRequestId.Should().Be(pullRequest.Id);
        });
    }

    [Fact]
    public void PullRequest_WithReviews_ShouldHaveReviews()
    {
        // Act
        var pullRequest = PullRequestDataBuilder.WithReviews(3);

        // Assert
        pullRequest.Reviews.Should().HaveCount(3);
        pullRequest.Reviews.Should().AllSatisfy(review =>
        {
            review.PullRequestId.Should().Be(pullRequest.Id);
        });
    }

    [Fact]
    public void PullRequestCollection_CreateMany_ShouldGenerateUniquePullRequests()
    {
        // Act
        var pullRequests = PullRequestDataBuilder.CreateMany(5);

        // Assert
        pullRequests.Should().HaveCount(5);
        pullRequests.Select(pr => pr.Id).Should().OnlyHaveUniqueItems();
        pullRequests.Select(pr => pr.Number).Should().OnlyHaveUniqueItems();
        pullRequests.Should().AllSatisfy(pr => pr.Id.Should().BeGreaterThan(0));
        pullRequests.Should().AllSatisfy(pr => pr.Title.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public void PullRequestCollection_CreateManyForRepository_ShouldAssignToRepository()
    {
        // Arrange
        var repositoryId = 12345L;

        // Act
        var pullRequests = PullRequestDataBuilder.CreateManyForRepository(repositoryId, 3);

        // Assert
        pullRequests.Should().HaveCount(3);
        pullRequests.Should().AllSatisfy(pr => pr.RepositoryId.Should().Be(repositoryId));
    }

    [Theory]
    [InlineData(PullRequestState.Open, false, false)]
    [InlineData(PullRequestState.Closed, true, false)]
    [InlineData(PullRequestState.Merged, true, true)]
    public void PullRequest_StateTransitions_ShouldHaveCorrectTimestamps(
        PullRequestState state, 
        bool shouldHaveClosedAt, 
        bool shouldHaveMergedAt)
    {
        // Act
        var pullRequest = new PullRequest
        {
            State = state,
            ClosedAt = shouldHaveClosedAt ? DateTimeOffset.Now : null,
            MergedAt = shouldHaveMergedAt ? DateTimeOffset.Now : null,
            MergedBy = shouldHaveMergedAt ? UserDataBuilder.Create() : null
        };

        // Assert
        if (shouldHaveClosedAt)
        {
            pullRequest.ClosedAt.Should().NotBeNull();
        }
        else
        {
            pullRequest.ClosedAt.Should().BeNull();
        }

        if (shouldHaveMergedAt)
        {
            pullRequest.MergedAt.Should().NotBeNull();
            pullRequest.MergedBy.Should().NotBeNull();
        }
        else
        {
            pullRequest.MergedAt.Should().BeNull();
            pullRequest.MergedBy.Should().BeNull();
        }
    }

    [Fact]
    public void PullRequest_Statistics_ShouldHaveNonNegativeValues()
    {
        // Act
        var pullRequest = PullRequestDataBuilder.Create();

        // Assert
        pullRequest.Commits.Should().BeGreaterOrEqualTo(0);
        pullRequest.Additions.Should().BeGreaterOrEqualTo(0);
        pullRequest.Deletions.Should().BeGreaterOrEqualTo(0);
        pullRequest.ChangedFiles.Should().BeGreaterOrEqualTo(0);
    }
}