# Troubleshooting Guide - GitHub PR Review Tool

This guide helps you resolve common issues and problems you might encounter while using the GitHub PR Review Tool.

## Table of Contents

1. [Installation Issues](#installation-issues)
2. [Authentication Problems](#authentication-problems)
3. [Performance Issues](#performance-issues)
4. [UI Problems](#ui-problems)
5. [Data Synchronization Issues](#data-synchronization-issues)
6. [Network and Connectivity](#network-and-connectivity)
7. [Platform-Specific Issues](#platform-specific-issues)
8. [Logging and Diagnostics](#logging-and-diagnostics)
9. [Getting Help](#getting-help)

## Installation Issues

### Windows Installation Problems

**MSIX Installation Fails**
- **Symptom**: "This app package is not supported for installation by App Installer"
- **Solution**: 
  1. Enable Developer Mode in Windows Settings
  2. Or install the certificate first: `Add-AppxPackage -Path certificate.cer`
  3. Try installing from PowerShell as Administrator

**PowerShell Script Blocked**
- **Symptom**: "Execution of scripts is disabled on this system"
- **Solution**:
  ```powershell
  Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
  ```

**Missing Dependencies**
- **Symptom**: Application won't start, missing .NET runtime
- **Solution**: 
  1. Download self-contained version instead
  2. Or install .NET 8.0 Desktop Runtime from Microsoft

### macOS Installation Problems

**App Won't Open - Security Warning**
- **Symptom**: "App is damaged and can't be opened"
- **Solution**:
  ```bash
  # Remove quarantine attribute
  xattr -dr com.apple.quarantine /Applications/GitHubPrTool.app
  
  # Or allow in System Preferences > Security & Privacy
  ```

**Gatekeeper Prevents Launch**
- **Symptom**: "App can't be opened because it is from an unidentified developer"
- **Solution**: 
  1. Right-click the app → Open
  2. Click "Open" in the dialog
  3. Or disable Gatekeeper temporarily:
     ```bash
     sudo spctl --master-disable
     ```

### Linux Installation Problems

**Permission Denied**
- **Symptom**: Cannot execute the application
- **Solution**:
  ```bash
  chmod +x GitHubPrTool.Desktop
  ```

**Missing GUI Libraries**
- **Symptom**: Application won't start on headless systems
- **Solution**: Install required GUI packages:
  ```bash
  # Ubuntu/Debian
  sudo apt install libgtk-3-0 libdrm2 libxrandr2 libasound2
  
  # CentOS/RHEL
  sudo yum install gtk3 libdrm libXrandr alsa-lib
  ```

## Authentication Problems

### OAuth Flow Issues

**Browser Doesn't Open**
- **Causes**: Default browser not set, firewall blocking
- **Solutions**:
  1. Manually copy the OAuth URL to your browser
  2. Set a default browser: `xdg-settings set default-web-browser firefox.desktop`
  3. Check firewall settings for port 8080

**Authentication Timeout**
- **Symptom**: "Authentication request timed out"
- **Solutions**:
  1. Complete OAuth flow within 10 minutes
  2. Check internet connectivity
  3. Verify system time is correct
  4. Try authentication again

**Token Refresh Fails**
- **Symptom**: Frequent re-authentication required
- **Solutions**:
  1. Clear stored credentials and re-authenticate
  2. Check GitHub token permissions
  3. Verify OAuth app configuration

### GitHub API Access Issues

**Rate Limit Exceeded**
- **Symptom**: "API rate limit exceeded"
- **Solutions**:
  1. Wait for rate limit reset (shown in error message)
  2. Use authenticated requests (login required)
  3. Reduce sync frequency in settings

**Repository Access Denied**
- **Symptom**: "Not Found" or "Forbidden" for repositories
- **Solutions**:
  1. Verify you have access to the repository
  2. Check if repository is private and you have permissions
  3. Re-authenticate to refresh permissions

## Performance Issues

### Slow Application Startup

**Long Initial Load Time**
- **Causes**: Large cache, network issues, antivirus scanning
- **Solutions**:
  1. Clear application cache
  2. Add application to antivirus exclusions
  3. Check network connectivity
  4. Move app to SSD if on mechanical drive

**High Memory Usage**
- **Causes**: Large number of cached repositories/PRs
- **Solutions**:
  1. Clear old cached data
  2. Reduce number of synced repositories
  3. Restart application periodically
  4. Increase virtual memory if needed

### Slow UI Response

**Laggy Interface**
- **Causes**: Hardware acceleration disabled, low memory
- **Solutions**:
  1. Enable hardware acceleration in display settings
  2. Close other memory-intensive applications  
  3. Update graphics drivers
  4. Reduce UI animation settings

**Slow List Scrolling**
- **Causes**: Large datasets, virtualization disabled
- **Solutions**:
  1. Use filters to reduce displayed items
  2. Enable list virtualization in settings
  3. Increase available RAM

## UI Problems

### Display Issues

**UI Elements Not Visible**
- **Causes**: High DPI scaling, theme issues
- **Solutions**:
  1. Adjust display scaling in OS settings
  2. Try different theme (Light/Dark)
  3. Reset window layout in settings
  4. Update graphics drivers

**Text Rendering Problems**
- **Causes**: Font rendering issues, DPI problems
- **Solutions**:
  1. Adjust font size in application settings
  2. Enable/disable font smoothing
  3. Try different UI scale factor
  4. Install missing system fonts

### Window Management

**Window Won't Resize**
- **Symptom**: Application window stuck at fixed size
- **Solutions**:
  1. Double-click title bar to toggle maximize
  2. Delete window layout settings file
  3. Reset UI settings to defaults

**Application Minimizes to System Tray**
- **Symptom**: Can't find application after minimizing
- **Solutions**:
  1. Check system tray area for application icon
  2. Use Alt+Tab to find the window
  3. Disable "minimize to tray" in settings

## Data Synchronization Issues

### Sync Failures

**Sync Process Hangs**
- **Symptom**: Synchronization never completes
- **Solutions**:
  1. Cancel sync and retry
  2. Check network connectivity
  3. Clear cache and force full sync
  4. Restart application

**Data Not Updating**
- **Symptom**: Comments/PRs don't reflect GitHub changes
- **Solutions**:
  1. Force manual refresh (F5)
  2. Check last sync timestamp
  3. Verify GitHub authentication
  4. Clear local cache

### Cache Problems

**Cache Corruption**
- **Symptom**: Application crashes or shows wrong data
- **Solutions**:
  1. Clear application cache directory
  2. Delete database file and resync
  3. Reset application to defaults

**Large Cache Size**
- **Symptom**: Application using too much disk space
- **Solutions**:
  1. Configure cache size limits
  2. Enable automatic cleanup
  3. Manually clear old cached data
  4. Archive less-used repositories

## Network and Connectivity

### Connection Issues

**No Internet Connection**
- **Symptom**: "Unable to connect to GitHub API"
- **Solutions**:
  1. Check internet connectivity
  2. Verify GitHub.com is accessible
  3. Check proxy settings if behind corporate firewall
  4. Use offline mode for cached data

**Proxy Configuration**
- **Symptom**: Connection fails in corporate environment
- **Solutions**:
  1. Configure proxy settings in application
  2. Set system proxy environment variables:
     ```bash
     export HTTP_PROXY=http://proxy.company.com:8080
     export HTTPS_PROXY=http://proxy.company.com:8080
     ```
  3. Add GitHub.com to proxy bypass list

### SSL/TLS Issues

**Certificate Validation Errors**
- **Symptom**: "SSL certificate validation failed"
- **Solutions**:
  1. Update system certificate store
  2. Check system date/time accuracy
  3. Disable SSL verification (not recommended for production)

## Platform-Specific Issues

### Windows-Specific

**Windows Defender Blocking**
- **Symptom**: Application deleted or quarantined
- **Solutions**:
  1. Add application to Windows Defender exclusions
  2. Restore file from quarantine
  3. Download from official source only

**Windows Update Breaks Application**
- **Symptom**: Application stops working after Windows update
- **Solutions**:
  1. Reinstall application
  2. Update to latest version
  3. Check for .NET runtime updates

### macOS-Specific

**App Translocation**
- **Symptom**: Application moved to random location
- **Solutions**:
  1. Move app to /Applications folder
  2. Run once from original download location
  3. Clear quarantine attributes

### Linux-Specific

**Wayland vs X11 Issues**
- **Symptom**: Application doesn't display correctly
- **Solutions**:
  1. Force X11 mode: `GDK_BACKEND=x11 ./GitHubPrTool.Desktop`
  2. Install XWayland compatibility layer
  3. Switch to X11 session temporarily

**Missing System Dependencies**
- **Symptom**: Application won't start
- **Solutions**:
  1. Install missing packages based on error messages
  2. Use package manager to install GUI libraries
  3. Try AppImage version for better compatibility

## Logging and Diagnostics

### Enable Debug Logging

**Application Logs**
Location varies by platform:
- Windows: `%APPDATA%\GitHubPrTool\logs\`
- macOS: `~/Library/Application Support/GitHubPrTool/logs/`
- Linux: `~/.config/GitHubPrTool/logs/`

**Enable Verbose Logging**
1. Edit application settings
2. Set log level to "Debug" or "Trace"
3. Restart application
4. Reproduce the issue
5. Check log files for errors

### Performance Monitoring

**Monitor Resource Usage**
```bash
# Windows
tasklist /FI "IMAGENAME eq GitHubPrTool.Desktop.exe"

# macOS/Linux
ps aux | grep GitHubPrTool
```

**Check Network Activity**
- Use network monitoring tools to verify API calls
- Check for excessive requests or timeouts
- Monitor bandwidth usage during sync

### Diagnostic Information

**System Information to Collect**
- Operating system version
- .NET runtime version
- Application version
- Available memory and disk space
- Network configuration
- Error messages and stack traces

**Generate Diagnostic Report**
The application can generate diagnostic reports including:
- System information
- Application settings
- Recent log entries
- Cache statistics
- Network connectivity status

## Getting Help

### Before Reporting Issues

1. **Search Existing Issues**: Check GitHub issues for similar problems
2. **Update Application**: Ensure you're using the latest version  
3. **Reproduce Issue**: Try to reproduce the problem consistently
4. **Collect Information**: Gather logs, screenshots, and system info

### Reporting Bugs

**GitHub Issues**: https://github.com/nam20485/pr-batch-comment-tool/issues

**Include in Bug Reports**:
- Steps to reproduce the issue
- Expected vs actual behavior
- Screenshots or screen recordings
- Log files (remove sensitive information)
- System information
- Application version

**Issue Template**:
```markdown
## Bug Description
Brief description of the issue

## Steps to Reproduce
1. Step one
2. Step two
3. Step three

## Expected Behavior
What you expected to happen

## Actual Behavior
What actually happened

## Environment
- OS: [Windows 11, macOS 13, Ubuntu 22.04]
- App Version: [1.0.0]
- .NET Version: [8.0.1]
- Other relevant info

## Additional Context
Any other relevant information, logs, or screenshots
```

### Community Support

- **GitHub Discussions**: For questions and general help
- **Stack Overflow**: Tag with `github-pr-tool`
- **Documentation**: Check user guide and API reference

### Emergency Contact

For critical security issues:
- Use GitHub's private vulnerability reporting at: https://github.com/nam20485/pr-batch-comment-tool/security/advisories/new
- Do not publicly disclose security vulnerabilities

---

If this guide doesn't resolve your issue, please don't hesitate to reach out through our support channels. We're here to help!