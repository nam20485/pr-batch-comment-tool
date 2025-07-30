using GitHubPrTool.Core.Models;

namespace GitHubPrTool.Core.Tests.Models;

/// <summary>
/// Comprehensive tests for the Repository domain model
/// </summary>
public class RepositoryTests
{
    [Fact]
    public void Repository_DefaultConstructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var repository = new Repository();

        // Assert
        Assert.Equal(0, repository.Id);
        Assert.Equal(string.Empty, repository.Name);
        Assert.Equal(string.Empty, repository.FullName);
        Assert.Null(repository.Description);
        Assert.Null(repository.HtmlUrl);
        Assert.Null(repository.CloneUrl);
        Assert.NotNull(repository.Owner);
        Assert.False(repository.Private);
        Assert.Equal("main", repository.DefaultBranch);
        Assert.Null(repository.Language);
        Assert.Equal(0, repository.StargazersCount);
        Assert.Equal(0, repository.ForksCount);
        Assert.Equal(0, repository.OpenIssuesCount);
        Assert.NotNull(repository.PullRequests);
        Assert.Empty(repository.PullRequests);
    }

    [Fact]
    public void Repository_SetProperties_ShouldRetainValues()
    {
        // Arrange
        var repository = new Repository();
        var owner = new User { Id = 123, Login = "testuser" };
        var now = DateTimeOffset.UtcNow;

        // Act
        repository.Id = 456;
        repository.Name = "test-repo";
        repository.FullName = "testuser/test-repo";
        repository.Description = "A test repository";
        repository.HtmlUrl = "https://github.com/testuser/test-repo";
        repository.CloneUrl = "https://github.com/testuser/test-repo.git";
        repository.Owner = owner;
        repository.Private = true;
        repository.DefaultBranch = "develop";
        repository.Language = "C#";
        repository.StargazersCount = 100;
        repository.ForksCount = 25;
        repository.OpenIssuesCount = 5;
        repository.CreatedAt = now;
        repository.UpdatedAt = now;
        repository.PushedAt = now;

        // Assert
        Assert.Equal(456, repository.Id);
        Assert.Equal("test-repo", repository.Name);
        Assert.Equal("testuser/test-repo", repository.FullName);
        Assert.Equal("A test repository", repository.Description);
        Assert.Equal("https://github.com/testuser/test-repo", repository.HtmlUrl);
        Assert.Equal("https://github.com/testuser/test-repo.git", repository.CloneUrl);
        Assert.Equal(owner, repository.Owner);
        Assert.True(repository.Private);
        Assert.Equal("develop", repository.DefaultBranch);
        Assert.Equal("C#", repository.Language);
        Assert.Equal(100, repository.StargazersCount);
        Assert.Equal(25, repository.ForksCount);
        Assert.Equal(5, repository.OpenIssuesCount);
        Assert.Equal(now, repository.CreatedAt);
        Assert.Equal(now, repository.UpdatedAt);
        Assert.Equal(now, repository.PushedAt);
    }

    [Fact]
    public void Repository_AddPullRequest_ShouldAddToCollection()
    {
        // Arrange
        var repository = new Repository { Id = 123 };
        var pullRequest = new PullRequest
        {
            Id = 456,
            Title = "Test PR",
            RepositoryId = 123
        };

        // Act
        repository.PullRequests.Add(pullRequest);

        // Assert
        Assert.Single(repository.PullRequests);
        Assert.Contains(pullRequest, repository.PullRequests);
    }
}

/// <summary>
/// Comprehensive tests for the PullRequest domain model
/// </summary>
public class PullRequestTests
{
    [Fact]
    public void PullRequest_DefaultConstructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var pullRequest = new PullRequest();

        // Assert
        Assert.Equal(0, pullRequest.Id);
        Assert.Equal(0, pullRequest.Number);
        Assert.Equal(string.Empty, pullRequest.Title);
        Assert.Null(pullRequest.Body);
        Assert.Equal(PullRequestState.Open, pullRequest.State);
        Assert.Null(pullRequest.HtmlUrl);
        Assert.NotNull(pullRequest.Author);
        Assert.NotNull(pullRequest.Repository);
        Assert.Equal(0, pullRequest.RepositoryId);
        Assert.Equal(string.Empty, pullRequest.BaseBranch);
        Assert.Equal(string.Empty, pullRequest.HeadBranch);
        Assert.False(pullRequest.IsDraft);
        Assert.Null(pullRequest.Mergeable);
        Assert.Equal(0, pullRequest.Commits);
        Assert.Equal(0, pullRequest.Additions);
        Assert.Equal(0, pullRequest.Deletions);
        Assert.Equal(0, pullRequest.ChangedFiles);
        Assert.Null(pullRequest.ClosedAt);
        Assert.Null(pullRequest.MergedAt);
        Assert.Null(pullRequest.MergedBy);
        Assert.NotNull(pullRequest.Reviews);
        Assert.Empty(pullRequest.Reviews);
        Assert.NotNull(pullRequest.Comments);
        Assert.Empty(pullRequest.Comments);
    }

    [Fact]
    public void PullRequest_SetState_ShouldAcceptAllValidStates()
    {
        // Arrange
        var pullRequest = new PullRequest();

        // Act & Assert
        pullRequest.State = PullRequestState.Open;
        Assert.Equal(PullRequestState.Open, pullRequest.State);

        pullRequest.State = PullRequestState.Closed;
        Assert.Equal(PullRequestState.Closed, pullRequest.State);

        pullRequest.State = PullRequestState.Merged;
        Assert.Equal(PullRequestState.Merged, pullRequest.State);
    }

    [Fact]
    public void PullRequest_AddComment_ShouldAddToCollection()
    {
        // Arrange
        var pullRequest = new PullRequest { Id = 123 };
        var comment = new Comment
        {
            Id = 456,
            Body = "Test comment",
            PullRequestId = 123
        };

        // Act
        pullRequest.Comments.Add(comment);

        // Assert
        Assert.Single(pullRequest.Comments);
        Assert.Contains(comment, pullRequest.Comments);
    }

    [Fact]
    public void PullRequest_AddReview_ShouldAddToCollection()
    {
        // Arrange
        var pullRequest = new PullRequest { Id = 123 };
        var review = new Review
        {
            Id = 789,
            Body = "Test review",
            PullRequestId = 123
        };

        // Act
        pullRequest.Reviews.Add(review);

        // Assert
        Assert.Single(pullRequest.Reviews);
        Assert.Contains(review, pullRequest.Reviews);
    }
}

/// <summary>
/// Comprehensive tests for the Comment domain model
/// </summary>
public class CommentTests
{
    [Fact]
    public void Comment_DefaultConstructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var comment = new Comment();

        // Assert
        Assert.Equal(0, comment.Id);
        Assert.Equal(string.Empty, comment.Body);
        Assert.NotNull(comment.Author);
        Assert.Equal(0, comment.PullRequestId);
        Assert.NotNull(comment.PullRequest); // Navigation property is initialized
        Assert.Null(comment.ReviewId);
        Assert.Null(comment.Review);
        Assert.Null(comment.Path);
        Assert.Null(comment.DiffHunk);
        Assert.Null(comment.Position);
        Assert.Null(comment.OriginalPosition);
        Assert.Null(comment.CommitId);
        Assert.Null(comment.OriginalCommitId);
        Assert.Null(comment.InReplyToId);
    }

    [Fact]
    public void Comment_SetProperties_ShouldRetainValues()
    {
        // Arrange
        var comment = new Comment();
        var author = new User { Id = 123, Login = "testuser" };
        var pullRequest = new PullRequest { Id = 456 };
        var now = DateTimeOffset.UtcNow;

        // Act
        comment.Id = 789;
        comment.Body = "This is a test comment";
        comment.Author = author;
        comment.PullRequestId = 456;
        comment.PullRequest = pullRequest;
        comment.Path = "src/test.cs";
        comment.DiffHunk = "@@ -1,3 +1,3 @@";
        comment.Position = 5;
        comment.OriginalPosition = 3;
        comment.CommitId = "abc123";
        comment.OriginalCommitId = "def456";
        comment.InReplyToId = 999;
        comment.CreatedAt = now;
        comment.UpdatedAt = now;

        // Assert
        Assert.Equal(789, comment.Id);
        Assert.Equal("This is a test comment", comment.Body);
        Assert.Equal(author, comment.Author);
        Assert.Equal(456, comment.PullRequestId);
        Assert.Equal(pullRequest, comment.PullRequest);
        Assert.Equal("src/test.cs", comment.Path);
        Assert.Equal("@@ -1,3 +1,3 @@", comment.DiffHunk);
        Assert.Equal(5, comment.Position);
        Assert.Equal(3, comment.OriginalPosition);
        Assert.Equal("abc123", comment.CommitId);
        Assert.Equal("def456", comment.OriginalCommitId);
        Assert.Equal(999, comment.InReplyToId);
        Assert.Equal(now, comment.CreatedAt);
        Assert.Equal(now, comment.UpdatedAt);
    }
}

/// <summary>
/// Comprehensive tests for the User domain model
/// </summary>
public class UserTests
{
    [Fact]
    public void User_DefaultConstructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        Assert.Equal(0, user.Id);
        Assert.Equal(string.Empty, user.Login);
        Assert.Null(user.Name);
        Assert.Null(user.Email);
        Assert.Null(user.AvatarUrl);
        Assert.Null(user.HtmlUrl);
        Assert.Null(user.Bio);
    }

    [Fact]
    public void User_SetProperties_ShouldRetainValues()
    {
        // Arrange
        var user = new User();
        var now = DateTimeOffset.UtcNow;

        // Act
        user.Id = 123;
        user.Login = "testuser";
        user.Name = "Test User";
        user.Email = "test@example.com";
        user.AvatarUrl = "https://github.com/avatar.jpg";
        user.HtmlUrl = "https://github.com/testuser";
        user.Bio = "A test user bio";
        user.CreatedAt = now;
        user.UpdatedAt = now;

        // Assert
        Assert.Equal(123, user.Id);
        Assert.Equal("testuser", user.Login);
        Assert.Equal("Test User", user.Name);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("https://github.com/avatar.jpg", user.AvatarUrl);
        Assert.Equal("https://github.com/testuser", user.HtmlUrl);
        Assert.Equal("A test user bio", user.Bio);
        Assert.Equal(now, user.CreatedAt);
        Assert.Equal(now, user.UpdatedAt);
    }
}