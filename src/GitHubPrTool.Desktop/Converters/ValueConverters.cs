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