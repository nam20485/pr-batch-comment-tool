using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using GitHubPrTool.Core.Models;

namespace GitHubPrTool.Desktop.Converters;

/// <summary>
/// Converter that converts pull request state to appropriate color brush.
/// </summary>
public class StateToColorConverter : IValueConverter
{
    /// <summary>
    /// Singleton instance of the converter.
    /// </summary>
    public static readonly StateToColorConverter Instance = new();

    /// <summary>
    /// Converts pull request state to color brush.
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is PullRequestState state)
        {
            return state switch
            {
                PullRequestState.Open => Brush.Parse("#22863a"), // Green
                PullRequestState.Closed => Brush.Parse("#d73a49"), // Red
                PullRequestState.Merged => Brush.Parse("#6f42c1"), // Purple
                _ => Brush.Parse("#586069") // Gray
            };
        }

        return Brush.Parse("#586069"); // Default gray
    }

    /// <summary>
    /// Not implemented for this converter.
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter that converts review state to appropriate color brush.
/// </summary>
public class ReviewStateToColorConverter : IValueConverter
{
    /// <summary>
    /// Singleton instance of the converter.
    /// </summary>
    public static readonly ReviewStateToColorConverter Instance = new();

    /// <summary>
    /// Converts review state to color brush.
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ReviewState state)
        {
            return state switch
            {
                ReviewState.Approved => Brush.Parse("#22863a"), // Green
                ReviewState.ChangesRequested => Brush.Parse("#d73a49"), // Red
                ReviewState.Commented => Brush.Parse("#0366d6"), // Blue
                ReviewState.Pending => Brush.Parse("#f66a0a"), // Orange
                ReviewState.Dismissed => Brush.Parse("#586069"), // Gray
                _ => Brush.Parse("#586069") // Gray
            };
        }

        return Brush.Parse("#586069"); // Default gray
    }

    /// <summary>
    /// Not implemented for this converter.
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter that converts comment type to appropriate color brush.
/// </summary>
public class CommentTypeToColorConverter : IValueConverter
{
    /// <summary>
    /// Singleton instance of the converter.
    /// </summary>
    public static readonly CommentTypeToColorConverter Instance = new();

    /// <summary>
    /// Converts comment type to color brush.
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is CommentType type)
        {
            return type switch
            {
                CommentType.Issue => Brush.Parse("#0366d6"), // Blue
                CommentType.Review => Brush.Parse("#22863a"), // Green
                CommentType.Commit => Brush.Parse("#f66a0a"), // Orange
                _ => Brush.Parse("#586069") // Gray
            };
        }

        return Brush.Parse("#586069"); // Default gray
    }

    /// <summary>
    /// Not implemented for this converter.
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter that checks equality between a value and a parameter.
/// </summary>
public class EqualityConverter : IValueConverter
{
    /// <summary>
    /// Singleton instance of the converter.
    /// </summary>
    public static readonly EqualityConverter Instance = new();

    /// <summary>
    /// Converts by comparing value with parameter.
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Equals(value, parameter);
    }

    /// <summary>
    /// Not implemented for this converter.
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter that converts boolean to selection mode text.
/// </summary>
public class BoolToSelectionTextConverter : IValueConverter
{
    /// <summary>
    /// Singleton instance of the converter.
    /// </summary>
    public static readonly BoolToSelectionTextConverter Instance = new();

    /// <summary>
    /// Converts boolean to selection text.
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelectionMode)
        {
            return isSelectionMode ? "✅ Exit Selection" : "☑️ Select Mode";
        }

        return "☑️ Select Mode";
    }

    /// <summary>
    /// Not implemented for this converter.
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter that converts DateTime to visibility (visible if different from default).
/// </summary>
public class DateTimeToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Singleton instance of the converter.
    /// </summary>
    public static readonly DateTimeToVisibilityConverter Instance = new();

    /// <summary>
    /// Converts DateTime to visibility.
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            // Show if the datetime is different from created time (indicating it was updated)
            return dateTime != default(DateTime);
        }

        return false;
    }

    /// <summary>
    /// Not implemented for this converter.
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter that converts integer to visibility (visible if greater than 0).
/// </summary>
public class IntToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Singleton instance of the converter.
    /// </summary>
    public static readonly IntToVisibilityConverter Instance = new();

    /// <summary>
    /// Converts integer to visibility.
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            return intValue > 0;
        }

        return false;
    }

    /// <summary>
    /// Not implemented for this converter.
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter that checks if a comment is selected.
/// </summary>
public class CommentSelectionConverter : IValueConverter
{
    /// <summary>
    /// Singleton instance of the converter.
    /// </summary>
    public static readonly CommentSelectionConverter Instance = new();

    /// <summary>
    /// Converts by checking if the comment is in the selected collection.
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is System.Collections.IEnumerable selectedComments && parameter is Comment comment)
        {
            foreach (var selectedComment in selectedComments)
            {
                if (selectedComment is Comment selected && selected.Id == comment.Id)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Not implemented for this converter.
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}