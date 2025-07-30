using Avalonia.Controls;
using Avalonia.Input;

namespace GitHubPrTool.Desktop.Views;

/// <summary>
/// Code-behind for the PullRequestListView user control.
/// </summary>
public partial class PullRequestListView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the PullRequestListView.
    /// </summary>
    public PullRequestListView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles the KeyDown event on the search box to trigger search on Enter key.
    /// </summary>
    /// <param name="sender">The search box.</param>
    /// <param name="e">Key event arguments.</param>
    private void SearchBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is ViewModels.PullRequestListViewModel viewModel)
        {
            // Search is automatic via property binding, but we can trigger a reload if needed
            // For now, just set focus away to trigger any pending updates
            if (sender is TextBox textBox && textBox.Parent is Panel parent)
            {
                parent.Focus();
            }
        }
    }
}