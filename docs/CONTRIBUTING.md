# Contributing to GitHub PR Review Tool

Thank you for your interest in contributing to the GitHub PR Review Tool! This document provides guidelines and information for contributors.

## Table of Contents

1. [Code of Conduct](#code-of-conduct)
2. [Getting Started](#getting-started)
3. [Development Process](#development-process)
4. [Pull Request Guidelines](#pull-request-guidelines)
5. [Coding Standards](#coding-standards)
6. [Testing Requirements](#testing-requirements)
7. [Documentation](#documentation)
8. [Issue Reporting](#issue-reporting)
9. [Community](#community)

## Code of Conduct

This project adheres to a code of conduct that we expect all contributors to follow. Please read and follow our [Code of Conduct](CODE_OF_CONDUCT.md) to help us maintain a welcoming and inclusive community.

### Our Pledge

We are committed to making participation in this project a harassment-free experience for everyone, regardless of age, body size, disability, ethnicity, gender identity and expression, level of experience, nationality, personal appearance, race, religion, or sexual identity and orientation.

## Getting Started

### Prerequisites

Before you begin contributing, make sure you have:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Git](https://git-scm.com/) for version control
- A GitHub account
- Familiarity with C# and .NET development
- Basic understanding of Avalonia UI (helpful but not required)

### Setting Up Your Development Environment

1. **Fork the Repository**
   ```bash
   # Fork the repo on GitHub, then clone your fork
   git clone https://github.com/YOUR_USERNAME/pr-batch-comment-tool.git
   cd pr-batch-comment-tool
   ```

2. **Add Upstream Remote**
   ```bash
   git remote add upstream https://github.com/nam20485/pr-batch-comment-tool.git
   ```

3. **Install Dependencies**
   ```bash
   dotnet restore
   ```

4. **Verify Setup**
   ```bash
   dotnet build
   dotnet test
   ```

5. **Run the Application**
   ```bash
   dotnet run --project src/GitHubPrTool.Desktop
   ```

### First Contribution

Looking for a good first issue? Check out issues labeled with:
- `good first issue` - Perfect for newcomers
- `help wanted` - We'd love community help on these
- `documentation` - Improve our docs
- `bug` - Fix a reported problem

## Development Process

### Workflow Overview

1. **Find or Create an Issue** - Discuss your proposed changes
2. **Create a Branch** - Work in a feature branch
3. **Make Changes** - Implement your feature or fix
4. **Test Thoroughly** - Ensure all tests pass
5. **Submit Pull Request** - Follow our PR template
6. **Address Feedback** - Work with maintainers to refine
7. **Celebrate** - Your contribution is merged! 🎉

### Branch Naming Convention

Use descriptive branch names that indicate the type of work:

```bash
# Features
feature/add-comment-templates
feature/improve-search-ui

# Bug fixes
fix/authentication-token-refresh
fix/comment-display-formatting

# Documentation
docs/update-installation-guide
docs/add-api-examples

# Refactoring
refactor/repository-pattern-cleanup
refactor/improve-error-handling
```

### Commit Message Guidelines

We follow [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
type(scope): brief description

[optional longer description]

[optional footer with breaking changes or issue references]
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation only changes
- `style`: Code style changes (formatting, semicolons, etc.)
- `refactor`: Code change that neither fixes a bug nor adds a feature
- `perf`: Performance improvement
- `test`: Adding missing tests or correcting existing tests
- `chore`: Changes to build process or auxiliary tools

**Examples:**
```bash
feat(auth): add GitHub OAuth integration
fix(ui): resolve comment list scrolling issue
docs(readme): update installation instructions
test(core): add unit tests for comment filtering
refactor(data): improve repository pattern implementation
```

## Pull Request Guidelines

### Before Submitting

- [ ] Create an issue to discuss your changes (for significant features)
- [ ] Fork the repository and create a feature branch
- [ ] Make sure all tests pass: `dotnet test`
- [ ] Build successfully: `dotnet build --configuration Release`
- [ ] Follow our coding standards
- [ ] Update documentation if needed
- [ ] Add tests for new functionality

### Pull Request Template

When creating a PR, please use this template:

```markdown
## Description
Brief description of the changes and which issue is fixed.

Fixes #(issue)

## Type of Change
- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update
- [ ] Performance improvement
- [ ] Code refactoring

## Testing
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual testing completed
- [ ] New tests added for new functionality

## Screenshots (if applicable)
Add screenshots to help explain your changes

## Checklist
- [ ] My code follows the project's style guidelines
- [ ] I have performed a self-review of my code
- [ ] I have commented my code, particularly in hard-to-understand areas
- [ ] I have made corresponding changes to the documentation
- [ ] My changes generate no new warnings
- [ ] Any dependent changes have been merged and published
```

### Review Process

1. **Automated Checks**: GitHub Actions will run tests and builds
2. **Code Review**: Maintainers will review your code
3. **Feedback**: Address any requested changes
4. **Approval**: At least one maintainer approval required
5. **Merge**: Maintainers will merge when ready

## Coding Standards

### C# Style Guidelines

We follow Microsoft's C# coding conventions with project-specific additions:

**Naming Conventions:**
```csharp
// Classes, methods, properties: PascalCase
public class GitHubService
{
    public async Task<Repository> GetRepositoryAsync(string name) { }
    public string RepositoryName { get; set; }
}

// Fields: camelCase with underscore prefix
private readonly IHttpClient _httpClient;
private bool _isInitialized;

// Parameters, local variables: camelCase
public void ProcessComments(List<Comment> commentList)
{
    var filteredComments = commentList.Where(c => c.IsActive);
}

// Constants: PascalCase
public const int MaxRetryAttempts = 3;
```

**File Organization:**
```csharp
// 1. Using statements (sort and group)
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GitHubPrTool.Core.Entities;
using GitHubPrTool.Core.Interfaces;

using Microsoft.Extensions.Logging;

// 2. Namespace
namespace GitHubPrTool.Infrastructure.Services;

// 3. Class with XML documentation
/// <summary>
/// Provides GitHub API integration services.
/// </summary>
public class GitHubService : IGitHubService
{
    // 4. Fields
    private readonly ILogger<GitHubService> _logger;
    
    // 5. Constructor
    public GitHubService(ILogger<GitHubService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    // 6. Properties
    public bool IsAuthenticated { get; private set; }
    
    // 7. Public methods
    // 8. Private methods
}
```

### XAML Style Guidelines

```xml
<!-- Use consistent indentation (4 spaces) -->
<UserControl x:Class="GitHubPrTool.Desktop.Views.CommentListView"
             xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- Group related properties -->
    <Grid Margin="8"
          RowDefinitions="Auto,*,Auto">
        
        <!-- Use descriptive names -->
        <TextBox Name="SearchTextBox"
                 Grid.Row="0"
                 Watermark="Search comments..."
                 Text="{Binding SearchText}" />
        
        <!-- Consistent spacing and alignment -->
        <ListBox Name="CommentsList"
                 Grid.Row="1"
                 Margin="0,8,0,8"
                 Items="{Binding Comments}"
                 SelectedItem="{Binding SelectedComment}" />
    
    </Grid>
</UserControl>
```

### Documentation Requirements

**XML Documentation:**
```csharp
/// <summary>
/// Retrieves pull request comments with optional filtering.
/// </summary>
/// <param name="owner">Repository owner username.</param>
/// <param name="repo">Repository name.</param>
/// <param name="pullNumber">Pull request number.</param>
/// <param name="filter">Optional filter criteria.</param>
/// <returns>A collection of filtered comments.</returns>
/// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
/// <exception cref="GitHubApiException">Thrown when API request fails.</exception>
public async Task<IEnumerable<Comment>> GetCommentsAsync(
    string owner, 
    string repo, 
    int pullNumber, 
    CommentFilter? filter = null)
{
    // Implementation
}
```

## Testing Requirements

### Test Categories

**Unit Tests** (Required for all new code):
- Test individual methods and classes
- Use mocking for dependencies
- Aim for >80% code coverage
- Fast execution (<1ms per test)

```csharp
[Fact]
public async Task GetCommentsAsync_WithValidParameters_ReturnsComments()
{
    // Arrange
    var service = new GitHubService(_mockHttpClient.Object);
    var expectedComments = CreateTestComments();
    
    // Act
    var result = await service.GetCommentsAsync("owner", "repo", 123);
    
    // Assert
    result.Should().NotBeNull();
    result.Should().HaveCount(expectedComments.Count);
}
```

**Integration Tests** (For complex features):
- Test component interactions
- Use test database
- Verify end-to-end scenarios

**UI Tests** (For UI changes):
- Test ViewModels
- Verify user interactions
- Use Avalonia testing framework

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific category
dotnet test --filter "Category=Unit"

# Watch mode for development
dotnet watch test
```

### Test Data and Utilities

Use the shared test utilities:

```csharp
// Use test builders for complex objects
var repository = new RepositoryBuilder()
    .WithName("test-repo")
    .WithOwner("test-owner")
    .Build();

// Use test data generators
var comments = TestDataGenerator.CreateComments(count: 5);
```

## Documentation

### Types of Documentation

1. **Code Documentation**: XML docs for public APIs
2. **User Documentation**: Guides and tutorials in `/docs`
3. **Developer Documentation**: Technical implementation details
4. **API Documentation**: Generated from XML docs

### Documentation Standards

- **Clear and Concise**: Write for your audience
- **Examples**: Include code examples where helpful
- **Up-to-Date**: Update docs when changing functionality
- **Screenshots**: Use images for UI documentation

### Building Documentation

```bash
# Generate API documentation
dotnet tool install -g docfx
docfx docs/docfx.json --serve
```

## Issue Reporting

### Bug Reports

When reporting bugs, please include:

1. **Environment Information**:
   - Operating System and version
   - .NET version
   - Application version

2. **Steps to Reproduce**:
   - Clear, numbered steps
   - Expected behavior
   - Actual behavior

3. **Additional Context**:
   - Screenshots or recordings
   - Log files (remove sensitive information)
   - Related issues or PRs

### Feature Requests

For feature requests, provide:

1. **Problem Description**: What problem does this solve?
2. **Proposed Solution**: How should it work?
3. **Alternatives Considered**: Other approaches you considered
4. **Additional Context**: Mockups, examples, use cases

### Security Issues

**DO NOT** create public issues for security vulnerabilities. Instead:

1. Email security concerns to [security contact]
2. Use GitHub's private vulnerability reporting
3. Allow time for assessment and patching
4. Coordinate disclosure timing

## Community

### Communication Channels

- **GitHub Issues**: Bug reports and feature requests
- **GitHub Discussions**: Questions and general discussion
- **Pull Requests**: Code review and collaboration

### Getting Help

- **Documentation**: Check our comprehensive docs first
- **Search Issues**: Your question might already be answered
- **Discussions**: Ask questions in GitHub Discussions
- **Code Review**: Learn from PR feedback

### Recognition

Contributors are recognized in:
- **Contributor Guide**: Listed in project documentation
- **Release Notes**: Mentioned in version releases
- **GitHub Contributors**: Automatically tracked by GitHub

## License

By contributing to this project, you agree that your contributions will be licensed under the same license as the project (MIT License).

---

Thank you for contributing to GitHub PR Review Tool! Your efforts help make this tool better for the entire community. 🚀