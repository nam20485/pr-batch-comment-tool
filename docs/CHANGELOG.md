# Changelog

All notable changes to the GitHub PR Review Tool will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Comprehensive documentation suite
- MSIX packaging for Windows distribution
- PowerShell installation script
- GitHub Actions CI/CD pipeline
- Cross-platform build scripts
- User guide and developer documentation

### Changed
- Updated project structure for better organization
- Enhanced README with detailed setup instructions

## [1.0.0] - 2024-01-01

### Added
- Initial release of GitHub PR Review Tool
- GitHub OAuth authentication
- Repository and Pull Request browsing
- Comment filtering and search functionality  
- Batch comment operations
- Offline caching capability
- Cross-platform Avalonia UI
- Dark theme support
- SQLite local database
- Entity Framework Core integration

### Core Features
- **Authentication**: Secure GitHub OAuth flow
- **Repository Management**: Browse accessible repositories
- **PR Navigation**: View and select pull requests
- **Comment Management**: Advanced filtering and sorting
- **Batch Operations**: Duplicate comment sets efficiently
- **Offline Support**: Work with cached data
- **Cross-Platform**: Windows, macOS, and Linux support

### Technical Implementation
- Clean Architecture with Core/Infrastructure/Desktop layers
- MVVM pattern with CommunityToolkit.Mvvm
- Repository pattern with local caching
- Comprehensive unit and integration tests
- Performance-optimized UI with virtualization

---

## Release Types

### Major Version (X.0.0)
- Breaking changes to public APIs
- Significant architecture changes
- Major new features that change user workflows
- Database schema changes requiring migration

### Minor Version (X.Y.0)
- New features that are backward compatible
- Significant enhancements to existing features
- New UI components or major UI improvements
- Performance improvements
- New platform support

### Patch Version (X.Y.Z)
- Bug fixes that are backward compatible
- Security patches
- Documentation updates
- Minor UI tweaks and improvements
- Dependency updates

---

## Versioning Strategy

This project follows [Semantic Versioning](https://semver.org/) principles:

- **MAJOR** version when you make incompatible API changes
- **MINOR** version when you add functionality in a backwards compatible manner  
- **PATCH** version when you make backwards compatible bug fixes

### Pre-release Versions
- **Alpha** (`1.0.0-alpha.1`): Early development, unstable
- **Beta** (`1.0.0-beta.1`): Feature complete, testing phase
- **RC** (`1.0.0-rc.1`): Release candidate, final testing

### Development Branches
- **main**: Production-ready code, tagged releases
- **develop**: Integration branch for features
- **feature/**: Feature development branches
- **hotfix/**: Critical bug fixes for production

### Release Process

1. **Development**: Features developed in feature branches
2. **Integration**: Merged to develop branch for testing
3. **Release Preparation**: Create release branch from develop
4. **Testing**: QA testing and bug fixes on release branch
5. **Release**: Merge to main, create tag, and deploy
6. **Hotfixes**: Direct fixes to main for critical issues

### Changelog Guidelines

#### Added
- New features
- New file additions
- New endpoints or APIs

#### Changed  
- Changes in existing functionality
- Updates to existing features
- Modifications to user interface

#### Deprecated
- Soon-to-be removed features
- Legacy functionality warnings

#### Removed
- Deleted features
- Removed files or endpoints
- Discontinued functionality

#### Fixed
- Bug fixes
- Error corrections
- Issue resolutions

#### Security
- Security improvements
- Vulnerability patches
- Security-related changes

### Commit Message Format

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
type(scope): description

[optional body]

[optional footer]
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix  
- `docs`: Documentation only changes
- `style`: Code style changes
- `refactor`: Code refactoring
- `perf`: Performance improvements
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

**Examples:**
```
feat(auth): add GitHub OAuth integration
fix(ui): resolve comment list scrolling issue
docs(readme): update installation instructions
perf(cache): optimize repository data loading
```

### Release Notes Template

Each release should include:

```markdown
## [Version] - Date

### 🚀 New Features
- Feature descriptions with benefits

### 🐛 Bug Fixes  
- Fixed issues with impact description

### 🔧 Improvements
- Performance and usability enhancements

### 📚 Documentation
- Documentation updates and additions

### 🔒 Security
- Security improvements and patches

### ⚠️ Breaking Changes
- Breaking changes with migration guidance

### 📦 Dependencies
- Updated dependencies and their versions
```

---

For detailed technical changes, see individual commit messages and pull request descriptions.