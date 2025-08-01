# Frequently Asked Questions (FAQ)

Common questions and answers about the GitHub PR Review Tool.

## Table of Contents

1. [General Questions](#general-questions)
2. [Installation & Setup](#installation--setup)
3. [Authentication & Security](#authentication--security)
4. [Features & Functionality](#features--functionality)
5. [Performance & Limitations](#performance--limitations)
6. [Troubleshooting](#troubleshooting)
7. [Development & Contributing](#development--contributing)

## General Questions

### What is the GitHub PR Review Tool?

The GitHub PR Review Tool is a cross-platform desktop application built with Avalonia UI that streamlines GitHub Pull Request review workflows. It allows you to efficiently manage and perform batch operations on PR comments, with offline capability and advanced filtering options.

### What platforms are supported?

The application supports:
- **Windows 10/11** (x64)
- **macOS 10.15+** (x64 and Apple Silicon via Rosetta)
- **Linux** distributions with GUI support (x64)

### Is this tool free to use?

Yes, the GitHub PR Review Tool is completely free and open-source under the MIT License.

### How does it differ from GitHub's web interface?

Key advantages include:
- **Batch Operations**: Select and duplicate multiple comments at once
- **Advanced Filtering**: More sophisticated comment filtering options
- **Offline Access**: Work with previously loaded data without internet
- **Performance**: Faster navigation with local caching
- **Desktop Integration**: Native desktop experience with keyboard shortcuts

## Installation & Setup

### How do I install the application?

**Windows:**
- Download and install the MSIX package (recommended)
- Or use PowerShell: `irm https://raw.githubusercontent.com/nam20485/pr-batch-comment-tool/main/scripts/install.ps1 | iex`
- Or extract the ZIP file and run the executable

**macOS:**
- Download the DMG file and drag to Applications
- Or extract the TAR.GZ file

**Linux:**
- Download and run the AppImage file
- Or extract the TAR.GZ file

### Do I need to install .NET separately?

No, all distributed packages are self-contained and include the required .NET 8.0 runtime.

### Can I run it from a USB drive?

Yes, the application is portable. Extract the ZIP/TAR.GZ version to your USB drive and run it directly.

### How do I update to a new version?

- **MSIX (Windows)**: Updates are handled automatically by Windows
- **Manual Installation**: Download and install the new version over the existing one
- **PowerShell Script**: Re-run the installation script to get the latest version

## Authentication & Security

### How does authentication work?

The application uses GitHub's OAuth2 flow:
1. Click "Sign in to GitHub"
2. Your browser opens to GitHub's authorization page  
3. Grant permissions to the application
4. You're redirected back and automatically authenticated

### What permissions does the app require?

The application requests:
- **Read access** to your repositories and pull requests
- **Write access** to create comments and reviews
- **Profile information** to identify you as the comment author

### Is my GitHub token stored securely?

Yes, authentication tokens are:
- Encrypted using system-provided security mechanisms
- Stored only locally on your device
- Never transmitted to third parties
- Automatically refreshed when needed

### Can I revoke access?

Yes, you can revoke access at any time:
- In the app: Account menu → Sign Out
- On GitHub: Settings → Applications → Authorized OAuth Apps

### Does the app work with GitHub Enterprise?

Currently, the app is designed for GitHub.com. GitHub Enterprise support is planned for future releases.

## Features & Functionality

### What is batch comment duplication?

This feature lets you:
1. Filter comments by author, file, or other criteria
2. Select multiple comments using checkboxes
3. Duplicate them as a new review from your account
4. Modify the comment text before submitting

This is useful for applying standard review feedback or copying another reviewer's comments.

### Can I work offline?

Yes, the application caches:
- Repository information
- Pull request data
- Comments and review history
- User profiles and metadata

You can browse and filter cached data without internet connection.

### How often does data sync with GitHub?

- **Manual Sync**: Press F5 or use the refresh button
- **Automatic Sync**: Configurable interval (default: 15 minutes)
- **Smart Sync**: Syncs when switching between PRs
- **Background Sync**: Updates data while you work

### Can I export comments or data?

Yes, you can export data in multiple formats:
- **CSV**: For spreadsheet analysis
- **JSON**: Complete data with metadata
- **Markdown**: Human-readable format
- **HTML**: For sharing or printing

### Does it support comment templates?

Yes, you can:
- Save frequently used comments as templates
- Organize templates by category
- Apply templates to new reviews
- Import/export template collections

### Can I filter comments by specific criteria?

Advanced filtering options include:
- **Author**: Show comments from specific users
- **File Path**: Filter by file or directory
- **Date Range**: Comments within specific timeframes
- **Status**: Active, outdated, or resolved comments
- **Content**: Search within comment text
- **Review State**: Comments from approved/rejected reviews

## Performance & Limitations

### How many repositories can I sync?

There's no hard limit, but performance considerations:
- **Recommended**: Up to 50 active repositories
- **Maximum tested**: 200+ repositories
- **Performance factors**: Number of PRs, comments, and sync frequency

### Are there API rate limits?

GitHub API limits:
- **Authenticated**: 5,000 requests per hour
- **Typical usage**: 10-50 requests per sync
- **Rate limit handling**: Automatic backoff and retry
- **Optimization**: Intelligent caching reduces API calls

### How much disk space does it use?

Typical storage usage:
- **Application**: 50-100 MB
- **Cache per repository**: 1-10 MB
- **Total for 20 repositories**: 100-300 MB
- **Configurable limits**: Set maximum cache size

### Can it handle large pull requests?

Yes, the application is optimized for:
- **Large comment threads**: Virtualized lists for performance
- **Many files**: Efficient filtering and navigation
- **Complex reviews**: Organized by conversation threads

### What's the maximum comment length?

Follows GitHub's limits:
- **Single comment**: 65,536 characters
- **Batch operations**: Limited by available memory
- **Export formats**: No additional restrictions

## Troubleshooting

### The app won't start - what should I check?

Common startup issues:
1. **Permissions**: Ensure the executable has run permissions
2. **Dependencies**: Use self-contained version if .NET issues
3. **Antivirus**: Add application to exclusions
4. **Logs**: Check log files for specific error messages

### Authentication keeps failing - how to fix?

Authentication troubleshooting:
1. **Browser**: Ensure default browser is set correctly
2. **Firewall**: Allow localhost connections on port 8080
3. **Time**: Verify system time is accurate
4. **Cache**: Clear browser cookies for GitHub
5. **Fresh start**: Sign out completely and re-authenticate

### The UI is too small/large - how to adjust?

UI scaling options:
1. **Application settings**: Adjust font size and UI scale
2. **System settings**: Change display scaling (Windows/macOS)
3. **Window management**: Reset window layout to defaults
4. **Theme**: Try different theme if rendering issues persist

### Data isn't syncing - what's wrong?

Sync troubleshooting:
1. **Authentication**: Verify you're still logged in
2. **Network**: Check internet connectivity
3. **Rate limits**: Wait if API limits exceeded
4. **Manual sync**: Force refresh with F5
5. **Cache**: Clear cache if data seems corrupted

### Can I reset all settings?

Yes, you can reset by:
1. **Settings menu**: Use "Reset to defaults" option
2. **Manual cleanup**: Delete application data directory
3. **Fresh install**: Uninstall and reinstall application

### Where are log files located?

Log file locations:
- **Windows**: `%APPDATA%\GitHubPrTool\logs\`
- **macOS**: `~/Library/Application Support/GitHubPrTool/logs/`
- **Linux**: `~/.config/GitHubPrTool/logs/`

## Development & Contributing

### How can I contribute to the project?

Ways to contribute:
- **Bug reports**: Report issues on GitHub
- **Feature requests**: Suggest improvements
- **Code contributions**: Submit pull requests
- **Documentation**: Improve guides and docs
- **Testing**: Help test new releases

### Is the source code available?

Yes, the project is open-source:
- **Repository**: https://github.com/nam20485/pr-batch-comment-tool
- **License**: MIT License
- **Build instructions**: See developer guide

### Can I build it myself?

Requirements for building:
- **.NET 8.0 SDK** or later
- **Git** for source control
- **IDE**: Visual Studio, VS Code, or Rider

Build steps:
```bash
git clone https://github.com/nam20485/pr-batch-comment-tool.git
cd pr-batch-comment-tool
dotnet restore
dotnet build
dotnet run --project src/GitHubPrTool.Desktop
```

### How do I request new features?

Feature request process:
1. **Search existing**: Check if already requested
2. **Create issue**: Use feature request template
3. **Describe use case**: Explain why it's needed
4. **Community feedback**: Others can vote and comment
5. **Development**: May be picked up by maintainers or contributors

### Can I create plugins or extensions?

Currently, the application has limited extensibility. Planned features:
- **Plugin system**: Load custom filters and exporters
- **Themes**: Custom UI themes
- **Scripts**: Automation and custom workflows

### How are releases managed?

Release process:
- **Semantic versioning**: Major.Minor.Patch format
- **Regular schedule**: Monthly minor releases
- **Hotfixes**: Critical issues addressed quickly
- **Pre-releases**: Beta versions for testing
- **LTS versions**: Long-term support for stable releases

### What's the development roadmap?

Upcoming features:
- **GitHub Enterprise support**
- **Advanced automation workflows**
- **Team collaboration features**
- **Integration with other tools**
- **Mobile companion app**

Check the GitHub project board for current development status.

---

## Still Have Questions?

If your question isn't answered here:

1. **Search the documentation**: Check user guide and developer docs
2. **GitHub Discussions**: Ask the community
3. **GitHub Issues**: Report bugs or request features
4. **Email support**: For private or sensitive questions

We're here to help make your code review process more efficient!