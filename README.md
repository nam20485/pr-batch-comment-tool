# GitHub PR Review Tool

A cross-platform desktop application built with Avalonia UI that streamlines GitHub Pull Request review workflows through intelligent comment management and batch operations.

![GitHub PR Review Tool](docs/images/app-screenshot.png)

## 🚀 Features

- **GitHub Integration**: Secure OAuth authentication with GitHub
- **Repository Management**: Browse and select from your accessible repositories
- **PR Navigation**: Intuitive pull request browsing and selection
- **Advanced Comment Filtering**: Filter comments by author, file, date, and status
- **Batch Operations**: Select and duplicate comment sets for efficient reviews
- **Offline Capability**: Work with previously loaded data without internet connectivity
- **Cross-Platform**: Runs on Windows, macOS, and Linux
- **Dark Theme**: Developer-friendly dark mode interface

## 📋 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Git](https://git-scm.com/) for version control
- GitHub account with appropriate repository access
- Windows 10/11, macOS 10.15+, or Linux with X11/Wayland

## 🛠️ Installation

### Option 1: Download Release (Recommended)

1. Go to the [Releases](https://github.com/nam20485/pr-batch-comment-tool/releases) page
2. Download the latest version for your platform:
   - **Windows**: `GitHubPrTool-win-x64.msix` or `GitHubPrTool-win-x64.zip`
   - **macOS**: `GitHubPrTool-osx-x64.tar.gz`
   - **Linux**: `GitHubPrTool-linux-x64.tar.gz`

### Option 2: Install via PowerShell (Windows)

```powershell
# Download and run the installation script
irm https://raw.githubusercontent.com/nam20485/pr-batch-comment-tool/main/scripts/install.ps1 | iex
```

### Option 3: Build from Source

```bash
# Clone the repository
git clone https://github.com/nam20485/pr-batch-comment-tool.git
cd pr-batch-comment-tool

# Restore dependencies
dotnet restore

# Build the application
dotnet build --configuration Release

# Run the application
dotnet run --project src/GitHubPrTool.Desktop
```

## 🎯 Quick Start

1. **Launch the Application**: Start GitHub PR Review Tool
2. **Authenticate**: Click "Sign in to GitHub" and complete OAuth flow
3. **Select Repository**: Choose a repository from your accessible list
4. **Browse Pull Requests**: Select a PR to review from the list
5. **Filter Comments**: Use the filter panel to narrow down comments
6. **Batch Operations**: Select comments and use "Duplicate Comments" for batch reviews

### First-Time Setup

1. **GitHub Token Configuration**: The app uses OAuth for secure authentication
2. **Data Storage**: Application data is stored locally in:
   - Windows: `%APPDATA%/GitHubPrTool`
   - macOS: `~/Library/Application Support/GitHubPrTool`
   - Linux: `~/.config/GitHubPrTool`

## 📖 User Guide

### Authentication
- **OAuth Flow**: Secure authentication without storing credentials
- **Token Management**: Automatic token refresh and secure storage
- **Offline Mode**: Previously loaded data remains accessible offline

### Repository Selection
- **Search**: Quickly find repositories using the search box
- **Filtering**: Filter by ownership (owned, member, collaborator)
- **Sorting**: Sort by name, updated date, or stars

### Pull Request Management
- **Status Filtering**: Filter by open, closed, merged, or draft status
- **Search**: Find PRs by title, number, or author
- **Sorting**: Sort by creation date, update date, or title

### Comment Operations
- **Advanced Filtering**: Filter by author, file path, outdated status
- **Bulk Selection**: Select multiple comments using checkboxes
- **Batch Duplication**: Copy selected comments as new review
- **Export/Import**: Save and load comment templates

### Keyboard Shortcuts
- `Ctrl+R` - Refresh current view
- `Ctrl+F` - Focus search box
- `Ctrl+A` - Select all filtered items
- `Ctrl+D` - Duplicate selected comments
- `F5` - Sync with GitHub (online mode)

## 🔧 Configuration

### Application Settings
Settings are stored in `settings.json` in the application data directory:

```json
{
  "theme": "dark",
  "autoRefreshInterval": 300,
  "defaultCommentFilters": {
    "includeOutdated": false,
    "includeResolved": true
  },
  "ui": {
    "windowSize": {
      "width": 1200,
      "height": 800
    },
    "splitPaneSizes": [300, 500, 400]
  }
}
```

### GitHub API Configuration
- **Rate Limiting**: Automatic rate limit handling and backoff
- **Caching**: Intelligent local caching reduces API calls
- **Sync Strategy**: Background synchronization with conflict resolution

## 🚀 Development

### Development Setup

```bash
# Clone and navigate to repository
git clone https://github.com/nam20485/pr-batch-comment-tool.git
cd pr-batch-comment-tool

# Restore packages and build
dotnet restore
dotnet build

# Run tests
dotnet test

# Start the application in development mode
dotnet run --project src/GitHubPrTool.Desktop
```

### Project Structure

```
├── src/
│   ├── GitHubPrTool.Core/           # Domain models and interfaces
│   ├── GitHubPrTool.Infrastructure/ # Data access and external services
│   └── GitHubPrTool.Desktop/        # Avalonia UI application
├── tests/
│   ├── GitHubPrTool.Core.Tests/
│   ├── GitHubPrTool.Infrastructure.Tests/
│   ├── GitHubPrTool.Desktop.Tests/
│   └── GitHubPrTool.TestUtilities/
├── docs/                            # Documentation
├── scripts/                         # Build and deployment scripts
└── .github/workflows/               # CI/CD pipelines
```

### Architecture
- **Clean Architecture**: Separation of concerns with clear dependency flow
- **MVVM Pattern**: Model-View-ViewModel for UI separation
- **Repository Pattern**: Abstracted data access with caching
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection

### Contributing
Please read [CONTRIBUTING.md](docs/CONTRIBUTING.md) for guidelines on contributing to this project.

## 📚 Documentation

- [User Manual](docs/user-guide.md) - Comprehensive user documentation
- [Developer Guide](docs/developer-guide.md) - Technical documentation for contributors
- [API Reference](docs/api-reference.md) - API documentation
- [Architecture Overview](docs/architecture.md) - System design and patterns
- [Release Notes](docs/CHANGELOG.md) - Version history and changes

## 🐛 Troubleshooting

### Common Issues

**Authentication Problems**
- Ensure GitHub OAuth app is properly configured
- Check firewall settings for localhost callbacks
- Verify system time is synchronized

**Performance Issues**
- Clear application cache: Delete data directory contents
- Check network connectivity for API calls
- Monitor system resources and available memory

**UI Issues**  
- Reset window layout: Delete `ui-layout.json` from data directory
- Update graphics drivers for rendering issues
- Try different theme settings

For more troubleshooting information, see [Troubleshooting Guide](docs/troubleshooting.md).

## 🤝 Support

- **Issues**: [GitHub Issues](https://github.com/nam20485/pr-batch-comment-tool/issues)
- **Discussions**: [GitHub Discussions](https://github.com/nam20485/pr-batch-comment-tool/discussions)
- **Wiki**: [Project Wiki](https://github.com/nam20485/pr-batch-comment-tool/wiki)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🏆 Acknowledgments

- [Avalonia UI](https://avaloniaui.net/) - Cross-platform .NET UI framework
- [Octokit.NET](https://github.com/octokit/octokit.net) - GitHub API client library
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) - Data access technology
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MVVM helpers

---

**Made with ❤️ for the GitHub developer community**