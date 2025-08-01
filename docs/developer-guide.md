# Developer Guide - GitHub PR Review Tool

This guide provides comprehensive technical documentation for developers contributing to the GitHub PR Review Tool project.

## Table of Contents

1. [Development Environment Setup](#development-environment-setup)
2. [Project Architecture](#project-architecture)
3. [Code Organization](#code-organization)
4. [Development Workflow](#development-workflow)
5. [Testing Strategy](#testing-strategy)
6. [Build and Deployment](#build-and-deployment)
7. [Contributing Guidelines](#contributing-guidelines)
8. [API Documentation](#api-documentation)
9. [Troubleshooting Development Issues](#troubleshooting-development-issues)

## Development Environment Setup

### Prerequisites

**Required Software**:
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Git](https://git-scm.com/) for version control
- IDE: [Visual Studio 2022](https://visualstudio.microsoft.com/), [Visual Studio Code](https://code.visualstudio.com/), or [JetBrains Rider](https://www.jetbrains.com/rider/)

**Recommended Tools**:
- [GitKraken](https://www.gitkraken.com/) or [GitHub Desktop](https://desktop.github.com/) for Git GUI
- [Postman](https://www.postman.com/) for API testing
- [DB Browser for SQLite](https://sqlitebrowser.org/) for database inspection

### Initial Setup

```bash
# Clone the repository
git clone https://github.com/nam20485/pr-batch-comment-tool.git
cd pr-batch-comment-tool

# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build

# Run tests to verify setup
dotnet test

# Launch the application
dotnet run --project src/GitHubPrTool.Desktop
```

### IDE Configuration

#### Visual Studio 2022
1. Open `GitHubPrTool.sln`
2. Install recommended extensions:
   - Avalonia for Visual Studio
   - EditorConfig Language Service
   - GitLen
3. Set startup project to `GitHubPrTool.Desktop`

#### Visual Studio Code
1. Open project root folder
2. Install recommended extensions (see `.vscode/extensions.json`):
   - C# extension
   - Avalonia for Visual Studio Code
   - EditorConfig for VS Code
3. Use `Ctrl+Shift+P` → ".NET: Generate Assets for Build and Debug"

### Environment Variables

Create a `.env` file in the project root for development:

```bash
# GitHub OAuth App Configuration (for development)
GITHUB_CLIENT_ID=your_development_client_id
GITHUB_CLIENT_SECRET=your_development_client_secret

# Database Configuration
DATABASE_PATH=./data/development.db
CONNECTION_STRING=Data Source=./data/development.db

# Logging Configuration
LOG_LEVEL=Debug
LOG_PATH=./logs/

# Feature Flags
ENABLE_TELEMETRY=false
ENABLE_CRASH_REPORTING=false
```

## Project Architecture

### Clean Architecture Overview

The project follows Clean Architecture principles with clear separation of concerns:

```
┌─────────────────────────────────────────┐
│              Presentation               │
│         (GitHubPrTool.Desktop)         │
├─────────────────────────────────────────┤
│             Application                 │
│    (ViewModels, Services, Commands)     │
├─────────────────────────────────────────┤
│               Domain                    │
│         (GitHubPrTool.Core)            │
├─────────────────────────────────────────┤
│            Infrastructure               │
│      (GitHubPrTool.Infrastructure)     │
└─────────────────────────────────────────┘
```

### Dependency Flow

- **Presentation** → **Application** → **Domain**
- **Infrastructure** → **Domain**
- No dependency from **Domain** to any other layer
- **Application** coordinates between **Presentation** and **Infrastructure**

### Key Design Patterns

1. **Repository Pattern**: Abstracts data access logic
2. **MVVM Pattern**: Separates UI logic from business logic
3. **Command Pattern**: Encapsulates user actions
4. **Observer Pattern**: Reactive UI updates
5. **Factory Pattern**: Creates complex objects
6. **Strategy Pattern**: Pluggable algorithms (e.g., sync strategies)

## Code Organization

### Source Structure

```
src/
├── GitHubPrTool.Core/              # Domain Layer
│   ├── Entities/                   # Domain entities
│   ├── Interfaces/                 # Domain interfaces
│   ├── ValueObjects/               # Value objects
│   ├── Exceptions/                 # Domain exceptions
│   └── Events/                     # Domain events
│
├── GitHubPrTool.Infrastructure/    # Infrastructure Layer
│   ├── Data/                       # Data access implementation
│   │   ├── Context/               # Entity Framework contexts
│   │   ├── Repositories/          # Repository implementations
│   │   └── Migrations/            # Database migrations
│   ├── Services/                   # External service implementations
│   │   ├── GitHub/                # GitHub API integration
│   │   ├── Storage/               # Local storage services
│   │   └── Sync/                  # Synchronization services
│   └── Configuration/              # Infrastructure configuration
│
└── GitHubPrTool.Desktop/           # Presentation Layer
    ├── ViewModels/                 # MVVM ViewModels
    ├── Views/                      # Avalonia Views (XAML + Code-behind)
    ├── Controls/                   # Custom UI controls
    ├── Converters/                 # Value converters
    ├── Services/                   # UI-specific services
    └── Assets/                     # Static resources (icons, images)
```

### Test Structure

```
tests/
├── GitHubPrTool.Core.Tests/        # Domain layer tests
├── GitHubPrTool.Infrastructure.Tests/ # Infrastructure tests
├── GitHubPrTool.Desktop.Tests/     # UI layer tests
└── GitHubPrTool.TestUtilities/     # Shared test utilities
```

### Naming Conventions

**C# Code**:
- **Classes**: PascalCase (`GitHubRepository`)
- **Methods**: PascalCase (`GetPullRequestsAsync`)
- **Properties**: PascalCase (`CommentText`)
- **Fields**: camelCase with underscore prefix (`_gitHubService`)
- **Constants**: PascalCase (`MaxRetryAttempts`)

**Files and Folders**:
- **Folders**: PascalCase (`ViewModels`)
- **Files**: PascalCase matching class name (`GitHubRepository.cs`)

**Database**:
- **Tables**: PascalCase (`PullRequests`)
- **Columns**: PascalCase (`CreatedAt`)
- **Foreign Keys**: `{Table}Id` (`PullRequestId`)

## Development Workflow

### Git Workflow

We use **GitHub Flow** with the following conventions:

1. **Main Branch**: Always deployable, protected
2. **Feature Branches**: `feature/short-description` or `feature/issue-number`
3. **Bug Fixes**: `bugfix/short-description` or `fix/issue-number`
4. **Releases**: `release/version-number`

### Commit Messages

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
type(scope): description

[optional body]

[optional footer]
```

**Types**:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

**Examples**:
```
feat(auth): add GitHub OAuth authentication
fix(ui): resolve comment list scrolling issue
docs(readme): update installation instructions
test(core): add unit tests for comment filtering
```

### Pull Request Process

1. **Create Branch**: From main branch
2. **Implement Changes**: Follow coding standards
3. **Write Tests**: Ensure adequate test coverage
4. **Update Documentation**: If public APIs change
5. **Self-Review**: Check your own changes
6. **Create PR**: Use the PR template
7. **Address Feedback**: Respond to review comments
8. **Merge**: Squash and merge when approved

### Code Review Guidelines

**For Authors**:
- Keep PRs focused and reasonably sized
- Provide clear description of changes
- Include screenshots for UI changes
- Ensure all tests pass
- Update documentation when needed

**For Reviewers**:
- Review within 24 hours when possible
- Focus on logic, design, and maintainability
- Be constructive and respectful
- Test locally for complex changes
- Approve when ready, request changes if needed

## Testing Strategy

### Test Types and Coverage

**Unit Tests** (Target: >80% coverage):
- Test individual classes and methods in isolation
- Mock external dependencies
- Fast execution (< 1ms per test)

**Integration Tests**:
- Test component interactions
- Use real database (in-memory SQLite)
- Test GitHub API integration with mock server

**UI Tests**:
- Test ViewModels and user interactions
- Use Avalonia's testing framework
- Mock business logic dependencies

**End-to-End Tests**:
- Test complete user workflows
- Use real GitHub test repositories
- Run in CI/CD pipeline

### Testing Tools and Frameworks

- **xUnit**: Primary testing framework
- **FluentAssertions**: Readable assertions
- **Moq**: Mocking framework
- **AutoFixture**: Test data generation
- **Respawn**: Database cleanup between tests
- **WireMock.NET**: HTTP service mocking

### Test Structure Example

```csharp
public class GitHubRepositoryServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactory;
    private readonly Mock<ITokenStorage> _tokenStorage;
    private readonly GitHubRepositoryService _service;

    public GitHubRepositoryServiceTests()
    {
        _httpClientFactory = new Mock<IHttpClientFactory>();
        _tokenStorage = new Mock<ITokenStorage>();
        _service = new GitHubRepositoryService(_httpClientFactory.Object, _tokenStorage.Object);
    }

    [Fact]
    public async Task GetRepositoriesAsync_WithValidToken_ReturnsRepositories()
    {
        // Arrange
        var expectedRepos = CreateTestRepositories();
        SetupHttpClient(expectedRepos);

        // Act
        var result = await _service.GetRepositoriesAsync();

        // Assert
        result.Should().HaveCount(expectedRepos.Count);
        result.Should().BeEquivalentTo(expectedRepos);
    }
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/GitHubPrTool.Core.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests matching filter
dotnet test --filter "Category=Integration"
```

## Build and Deployment

### Local Build

```bash
# Debug build
dotnet build

# Release build
dotnet build --configuration Release

# Publish for specific platform
dotnet publish -c Release -r win-x64 --self-contained true
dotnet publish -c Release -r osx-x64 --self-contained true
dotnet publish -c Release -r linux-x64 --self-contained true
```

### CI/CD Pipeline

The project uses GitHub Actions for continuous integration:

**Build Workflow** (`.github/workflows/build.yml`):
- Triggered on push/PR to main
- Builds on Windows, macOS, Linux
- Runs all tests with coverage
- Uploads build artifacts

**Release Workflow** (`.github/workflows/release.yml`):
- Triggered on tag push (v*.*.*)
- Creates platform-specific packages
- Publishes to GitHub Releases
- Updates package managers

### Package Creation

**Windows (MSIX)**:
```xml
<!-- In GitHubPrTool.Desktop.csproj -->
<PropertyGroup>
  <WindowsPackageType>MSIX</WindowsPackageType>
  <WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
</PropertyGroup>
```

**macOS (DMG)**:
```bash
# Create app bundle
dotnet publish -c Release -r osx-x64
# Create DMG using create-dmg tool
```

**Linux (AppImage)**:
```bash
# Use linuxdeploy to create AppImage
dotnet publish -c Release -r linux-x64 --self-contained true
```

## Contributing Guidelines

### Code Style

We follow Microsoft's C# coding conventions with some project-specific rules:

**EditorConfig** (`.editorconfig`):
```ini
root = true

[*.cs]
indent_style = space
indent_size = 4
end_of_line = crlf
insert_final_newline = true
trim_trailing_whitespace = true

# .NET coding conventions
dotnet_style_qualification_for_field = false:suggestion
dotnet_style_qualification_for_property = false:suggestion
```

### Documentation Standards

**XML Documentation**:
- All public APIs must have XML docs
- Include `<summary>`, `<param>`, `<returns>` tags
- Provide usage examples for complex methods

**README Files**:
- Each major component should have a README
- Keep documentation up-to-date with code changes
- Include diagrams for complex architectures

### Performance Guidelines

**Async/Await**:
- Use async/await for I/O operations
- Configure await with `ConfigureAwait(false)` in library code
- Avoid async void except for event handlers

**Memory Management**:
- Dispose of resources properly
- Use `using` statements for IDisposable objects
- Avoid unnecessary allocations in hot paths

**UI Performance**:
- Use virtualization for large lists
- Implement lazy loading for expensive operations
- Minimize work on UI thread

## API Documentation

### Core Interfaces

#### IRepository<T>
Generic repository interface for data access:

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}
```

#### IGitHubService
GitHub API integration interface:

```csharp
public interface IGitHubService
{
    Task<IEnumerable<Repository>> GetRepositoriesAsync();
    Task<IEnumerable<PullRequest>> GetPullRequestsAsync(string owner, string repo);
    Task<IEnumerable<Comment>> GetCommentsAsync(string owner, string repo, int pullNumber);
    Task<Comment> CreateCommentAsync(string owner, string repo, int pullNumber, string body);
}
```

### ViewModels

#### MainWindowViewModel
Primary application ViewModel:

```csharp
public class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private Repository? selectedRepository;

    [ObservableProperty]
    private PullRequest? selectedPullRequest;

    [RelayCommand]
    private async Task LoadRepositoriesAsync()
    {
        // Implementation
    }
}
```

### Events and Messaging

The application uses `CommunityToolkit.Mvvm` messaging:

```csharp
// Send message
WeakReferenceMessenger.Default.Send(new RepositorySelectedMessage(repository));

// Receive message
[RelayCommand]
private void OnRepositorySelected(RepositorySelectedMessage message)
{
    SelectedRepository = message.Repository;
}
```

## Troubleshooting Development Issues

### Common Build Issues

**Package Restore Failures**:
- Clear NuGet cache: `dotnet nuget locals all --clear`
- Delete `bin` and `obj` folders
- Run `dotnet restore --force`

**Avalonia Designer Issues**:
- Restart IDE
- Clean and rebuild solution
- Check Avalonia extension is installed

### Runtime Issues

**Database Migration Problems**:
```bash
# Reset database
rm -f data/app.db
dotnet ef database update --project src/GitHubPrTool.Infrastructure
```

**GitHub API Rate Limiting**:
- Use personal access token for development
- Implement proper retry logic
- Cache responses when possible

### Debugging Tips

**Logging Configuration**:
```csharp
// In Program.cs
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Debug);
});
```

**Performance Profiling**:
- Use Visual Studio Diagnostic Tools
- Add performance counters for critical paths
- Monitor memory usage patterns

### Getting Help

- **GitHub Issues**: Report bugs and request features
- **GitHub Discussions**: Ask questions and share ideas
- **Code Reviews**: Get feedback on implementations
- **Documentation**: Contribute to project documentation

---

For more detailed technical information, see the [Architecture Documentation](architecture.md) and [API Reference](api-reference.md).