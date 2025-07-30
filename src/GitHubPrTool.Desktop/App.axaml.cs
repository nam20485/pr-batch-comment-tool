using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GitHubPrTool.Desktop.Views;
using GitHubPrTool.Desktop.ViewModels;
using GitHubPrTool.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GitHubPrTool.Desktop;

/// <summary>
/// Main application class for the GitHub PR Review Assistant Desktop Application.
/// Handles application startup, dependency injection setup, and lifetime management.
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    /// <summary>
    /// Initializes the application and sets up XAML resources.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Called when the application framework initialization is complete.
    /// Sets up dependency injection and the main window.
    /// </summary>
    /// <param name="e">Application lifetime event arguments.</param>
    public override void OnFrameworkInitializationCompleted()
    {
        // Build the host with dependency injection
        _host = CreateHostBuilder().Build();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Get the main window and its view model from DI container
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            var mainViewModel = _host.Services.GetRequiredService<MainWindowViewModel>();
            
            mainWindow.DataContext = mainViewModel;
            desktop.MainWindow = mainWindow;
        }

        // Setup application lifetime event handlers
        SetupApplicationLifetime();

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Creates and configures the host builder with all necessary services.
    /// </summary>
    /// <returns>Configured host builder.</returns>
    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Add infrastructure services (repositories, caching, GitHub API)
                services.AddGitHubPrToolServices(context.Configuration, context.HostingEnvironment);
                
                // Add Avalonia UI services
                services.AddUIServices();
            });
    }

    /// <summary>
    /// Cleanup when application is shutting down.
    /// Subscribe to the application lifetime exit event for proper cleanup.
    /// </summary>
    private void SetupApplicationLifetime()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.ShutdownRequested += (sender, e) =>
            {
                _host?.Dispose();
            };
        }
    }
}

/// <summary>
/// Extension methods for configuring UI services in dependency injection.
/// </summary>
public static class UIServiceExtensions
{
    /// <summary>
    /// Adds UI-specific services to the dependency injection container.
    /// </summary>
    /// <param name="services">Service collection to add services to.</param>
    /// <returns>Service collection for method chaining.</returns>
    public static IServiceCollection AddUIServices(this IServiceCollection services)
    {
        // Register Views
        services.AddTransient<MainWindow>();
        
        // Register ViewModels
        services.AddTransient<MainWindowViewModel>();
        
        // Navigation service will be added here later
        // services.AddSingleton<INavigationService, NavigationService>();
        
        return services;
    }
}