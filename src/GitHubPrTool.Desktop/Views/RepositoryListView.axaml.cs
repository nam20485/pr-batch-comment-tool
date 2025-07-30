using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace GitHubPrTool.Desktop.Views;

/// <summary>
/// Code-behind for the RepositoryListView user control.
/// </summary>
public partial class RepositoryListView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the RepositoryListView.
    /// </summary>
    public RepositoryListView()
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
        if (e.Key == Key.Enter && DataContext is ViewModels.RepositoryListViewModel viewModel)
        {
            // Trigger search when Enter is pressed
            viewModel.SearchRepositoriesCommand.Execute(null);
        }
    }
}