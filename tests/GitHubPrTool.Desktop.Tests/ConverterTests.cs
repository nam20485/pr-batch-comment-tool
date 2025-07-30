using FluentAssertions;
using GitHubPrTool.Core.Models;
using GitHubPrTool.Desktop.Converters;
using System.Collections.ObjectModel;
using System.Globalization;

namespace GitHubPrTool.Desktop.Tests;

/// <summary>
/// Tests for value converters.
/// </summary>
public class ConverterTests
{
    [Fact]
    public void CommentSelectionConverter_WithCommentInCollection_ShouldReturnTrue()
    {
        // Arrange
        var converter = CommentSelectionConverter.Instance;
        var comment = new Comment { Id = 1, Body = "Test comment", Author = new User { Login = "user" } };
        var selectedComments = new ObservableCollection<Comment> { comment };

        // Act
        var result = converter.Convert(selectedComments, typeof(bool), comment, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void CommentSelectionConverter_WithCommentNotInCollection_ShouldReturnFalse()
    {
        // Arrange
        var converter = CommentSelectionConverter.Instance;
        var comment1 = new Comment { Id = 1, Body = "Test comment 1", Author = new User { Login = "user1" } };
        var comment2 = new Comment { Id = 2, Body = "Test comment 2", Author = new User { Login = "user2" } };
        var selectedComments = new ObservableCollection<Comment> { comment1 };

        // Act
        var result = converter.Convert(selectedComments, typeof(bool), comment2, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void CommentSelectionConverter_WithEmptyCollection_ShouldReturnFalse()
    {
        // Arrange
        var converter = CommentSelectionConverter.Instance;
        var comment = new Comment { Id = 1, Body = "Test comment", Author = new User { Login = "user" } };
        var selectedComments = new ObservableCollection<Comment>();

        // Act
        var result = converter.Convert(selectedComments, typeof(bool), comment, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void CommentSelectionConverter_WithNullValues_ShouldReturnFalse()
    {
        // Arrange
        var converter = CommentSelectionConverter.Instance;

        // Act
        var result = converter.Convert(null, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void CommentSelectionConverter_ConvertBack_ShouldThrowNotImplementedException()
    {
        // Arrange
        var converter = CommentSelectionConverter.Instance;

        // Act & Assert
        converter.Invoking(c => c.ConvertBack(true, typeof(bool), null, CultureInfo.InvariantCulture))
                 .Should().Throw<NotImplementedException>();
    }
}