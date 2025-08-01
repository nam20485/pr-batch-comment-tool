# User Guide - GitHub PR Review Tool

This comprehensive guide will help you master the GitHub PR Review Tool and streamline your code review workflows.

## Table of Contents

1. [Getting Started](#getting-started)
2. [Authentication Setup](#authentication-setup)
3. [Repository Management](#repository-management)
4. [Pull Request Navigation](#pull-request-navigation)
5. [Comment Management](#comment-management)
6. [Batch Operations](#batch-operations)
7. [Advanced Features](#advanced-features)
8. [Keyboard Shortcuts](#keyboard-shortcuts)
9. [Settings and Configuration](#settings-and-configuration)
10. [Troubleshooting](#troubleshooting)

## Getting Started

### First Launch

When you first launch GitHub PR Review Tool, you'll be presented with the welcome screen. The application follows a simple workflow:

1. **Authenticate** with your GitHub account
2. **Select** a repository to work with
3. **Choose** a pull request to review
4. **Filter and manage** comments efficiently
5. **Perform batch operations** to streamline reviews

### Interface Overview

The application uses a three-pane layout:

- **Left Pane**: Repository list and navigation
- **Center Pane**: Pull request list and details
- **Right Pane**: Comment list and management tools

## Authentication Setup

### GitHub OAuth Configuration

1. Click **"Sign in to GitHub"** on the welcome screen
2. Your default browser will open to GitHub's OAuth page
3. **Authorize** the application to access your repositories
4. The browser will redirect back to the application
5. You'll see a **success message** when authentication is complete

### Token Management

- **Automatic Refresh**: Tokens are automatically refreshed when needed
- **Secure Storage**: Tokens are encrypted and stored locally
- **Logout**: Use "Account → Sign Out" to revoke access

### Troubleshooting Authentication

**Common Issues:**
- **Browser doesn't open**: Check your default browser settings
- **Redirect fails**: Ensure localhost callbacks aren't blocked by firewall
- **Token expired**: Re-authenticate through the Account menu

## Repository Management

### Browsing Repositories

The repository list shows all repositories you have access to:

- **Owned**: Repositories you own
- **Member**: Organization repositories where you're a member
- **Collaborator**: Repositories where you're an external collaborator

### Search and Filtering

**Search Bar**: Type to find repositories by name
**Filter Options**:
- **All**: Show all accessible repositories
- **Owner**: Show only repositories you own
- **Member**: Show only organization repositories
- **Starred**: Show only starred repositories

**Sorting Options**:
- **Name** (A-Z)
- **Last Updated** (Most recent first)
- **Stars** (Most starred first)
- **Created Date** (Newest first)

### Repository Information

Each repository entry displays:
- **Name** and description
- **Language** and star count
- **Last updated** timestamp
- **Privacy status** (public/private)

## Pull Request Navigation

### PR List View

After selecting a repository, you'll see all pull requests:

**Status Indicators**:
- 🟢 **Open**: Active pull requests
- 🔴 **Closed**: Closed without merging
- 🟣 **Merged**: Successfully merged PRs
- 📝 **Draft**: Work-in-progress PRs

### Filtering Pull Requests

**Status Filter**: Choose which PR states to display
**Author Filter**: Filter by PR author
**Label Filter**: Filter by assigned labels
**Date Range**: Filter by creation or update date

### PR Information Display

Each pull request shows:
- **Title** and number (#123)
- **Author** and creation date
- **Status** and merge information
- **Comment count** and review status
- **File changes** summary

## Comment Management

### Comment List View

The comment pane displays all review comments for the selected PR:

**Comment Information**:
- **Author** name and avatar
- **Timestamp** of creation
- **File location** (file:line)
- **Comment content** with formatting
- **Status** (active, outdated, resolved)

### Comment Filtering

**Advanced Filters**:
- **Author**: Show comments by specific users
- **File Path**: Filter by file or directory
- **Status**: Include/exclude outdated or resolved comments
- **Date Range**: Filter by comment date
- **Content Search**: Search within comment text

**Quick Filters**:
- **My Comments**: Show only your comments
- **Unresolved**: Show only unresolved conversations
- **Recent**: Show comments from last 24 hours

### Comment Status Indicators

- ✅ **Resolved**: Conversation marked as resolved
- 🔄 **Outdated**: Comment on old version of code
- 💬 **Active**: Current, unresolved comment
- 📝 **Draft**: Pending review comment

## Batch Operations

### Selecting Comments

**Individual Selection**: Click checkboxes next to comments
**Bulk Selection**: 
- **Select All**: Use Ctrl+A or "Select All" button
- **Select Filtered**: Select all currently visible comments
- **Select by Author**: Select all comments by specific user

### Duplication Operations

**Duplicate Comments**:
1. **Filter** to find source comments (e.g., by author)
2. **Select** comments you want to duplicate
3. **Click** "Duplicate Selected Comments"
4. **Review** the generated comment text
5. **Submit** as a new review

**Customization Options**:
- **Modify content** before submitting
- **Add prefix/suffix** to duplicated comments
- **Change target files** if needed
- **Set review status** (approve, request changes, comment)

### Template Management

**Save Templates**:
- Select frequently used comments
- Save as template with custom name
- Templates stored locally for reuse

**Load Templates**:
- Browse saved templates
- Apply to current PR
- Customize before submitting

## Advanced Features

### Offline Mode

**Cache Management**:
- Previously loaded data remains available offline
- Sync when connection restored
- Manual refresh to get latest updates

**Offline Capabilities**:
- Browse cached repositories and PRs
- View and filter cached comments
- Prepare comments for later submission

### Export and Import

**Export Options**:
- **CSV**: Comment data for spreadsheet analysis
- **JSON**: Full comment structure with metadata
- **Markdown**: Human-readable format

**Import Features**:
- Import comment templates
- Bulk import from external sources
- Merge with existing data

### Data Synchronization

**Sync Strategies**:
- **Manual**: Sync only when requested
- **Automatic**: Background sync every N minutes
- **Smart**: Sync when switching between PRs

**Conflict Resolution**:
- **Server Wins**: Always use GitHub data
- **Local Wins**: Preserve local changes
- **Merge**: Intelligent conflict resolution

## Keyboard Shortcuts

### Navigation
- `Ctrl+1` - Focus repository list
- `Ctrl+2` - Focus pull request list
- `Ctrl+3` - Focus comment list
- `Tab` - Navigate between panes
- `F5` - Refresh current view

### Search and Filter
- `Ctrl+F` - Focus search box
- `Ctrl+Shift+F` - Advanced filter dialog
- `Escape` - Clear filters
- `Ctrl+H` - Toggle filter panel

### Selection and Operations
- `Ctrl+A` - Select all items in current list
- `Ctrl+Shift+A` - Select all filtered items
- `Ctrl+D` - Duplicate selected comments
- `Ctrl+E` - Export selected items
- `Delete` - Remove selected items (where applicable)

### Application
- `Ctrl+R` - Refresh and sync with GitHub
- `Ctrl+,` - Open settings
- `Ctrl+Q` - Quit application
- `F1` - Show help documentation

## Settings and Configuration

### General Settings

**Theme Options**:
- **Dark Mode**: Optimized for low-light environments
- **Light Mode**: Traditional light interface
- **System**: Follow system theme preferences

**Language Settings**:
- **Interface Language**: Application UI language
- **Date Format**: Regional date/time formatting
- **Number Format**: Regional number formatting

### Synchronization Settings

**Auto-Sync Configuration**:
- **Interval**: How often to sync (5min - 1hour)
- **Background Sync**: Sync while app is minimized
- **Sync on Startup**: Automatically sync when launching

**Cache Settings**:
- **Cache Size**: Maximum local storage usage
- **Retention Period**: How long to keep cached data
- **Cleanup Policy**: Automatic cleanup of old data

### UI Preferences

**Layout Options**:
- **Pane Sizes**: Adjust relative pane widths
- **Font Size**: Adjust interface font size
- **Compact Mode**: Reduce spacing for smaller screens

**Display Settings**:
- **Show Avatars**: Display user profile pictures
- **Relative Dates**: Show "2 hours ago" vs exact timestamps
- **File Icons**: Show file type icons in comment list

### Advanced Settings

**GitHub API**:
- **Rate Limit Handling**: How to handle API limits
- **Request Timeout**: Network request timeout duration
- **Retry Policy**: Failed request retry configuration

**Security Settings**:
- **Token Storage**: How authentication tokens are stored
- **Auto-Lock**: Automatically lock application when idle
- **Data Encryption**: Encrypt local database

## Troubleshooting

### Performance Issues

**Slow Loading**:
- Check internet connection speed
- Verify GitHub API status
- Clear application cache
- Reduce sync frequency

**High Memory Usage**:
- Limit number of loaded repositories
- Clear old cached data
- Restart application periodically
- Check for memory leaks in logs

### Synchronization Problems

**Sync Failures**:
- Verify GitHub token is valid
- Check API rate limit status
- Review network connectivity
- Examine error logs

**Data Conflicts**:
- Use "Force Sync" to reload from GitHub
- Check for conflicting local changes
- Review conflict resolution settings

### UI Issues

**Display Problems**:
- Update graphics drivers
- Try different theme settings
- Reset window layout
- Check display scaling settings

**Responsiveness Issues**:
- Close unnecessary applications
- Increase virtual memory
- Check for background processes
- Restart application

### Getting Help

**Log Files**: Located in application data directory
**Error Reports**: Automatic crash reporting (opt-in)
**Support Channels**:
- GitHub Issues for bug reports
- GitHub Discussions for questions
- Wiki for community documentation

---

For additional help, visit our [GitHub repository](https://github.com/nam20485/pr-batch-comment-tool) or check the [FAQ section](faq.md).