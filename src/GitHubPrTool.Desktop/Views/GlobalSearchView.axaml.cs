using Avalonia.Controls;
using GitHubPrTool.Desktop.ViewModels;

namespace GitHubPrTool.Desktop.Views;

/// <summary>
/// View for global search functionality across all data types
/// </summary>
public partial class GlobalSearchView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the GlobalSearchView
    /// </summary>
    public GlobalSearchView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Sets the view model and wires up any additional event handlers
    /// </summary>
    /// <param name="viewModel">The GlobalSearchViewModel to bind to this view</param>
    public void SetViewModel(GlobalSearchViewModel viewModel)
    {
        DataContext = viewModel;
    }
}