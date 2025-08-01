# GitHub PR Review Tool - Installation Script
# This script downloads and installs the latest version of GitHub PR Review Tool

param(
    [string]$Version = "latest",
    [string]$InstallPath = "$env:LOCALAPPDATA\Programs\GitHubPrTool",
    [switch]$Force,
    [switch]$Quiet
)

$ErrorActionPreference = "Stop"

# Script configuration
$RepoOwner = "nam20485"
$RepoName = "pr-batch-comment-tool"
$AppName = "GitHub PR Review Tool"
$ExeName = "GitHubPrTool.Desktop.exe"

# Colors for output
$Colors = @{
    Success = "Green"
    Warning = "Yellow"
    Error = "Red"
    Info = "Cyan"
}

function Write-ColorOutput {
    param([string]$Message, [string]$Color = "White")
    if (-not $Quiet) {
        Write-Host $Message -ForegroundColor $Colors[$Color]
    }
}

function Test-Administrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Get-LatestRelease {
    Write-ColorOutput "Fetching latest release information..." "Info"
    
    try {
        $uri = "https://api.github.com/repos/$RepoOwner/$RepoName/releases/latest"
        $response = Invoke-RestMethod -Uri $uri -Headers @{"Accept" = "application/vnd.github.v3+json"}
        return $response
    }
    catch {
        Write-ColorOutput "Failed to fetch release information: $($_.Exception.Message)" "Error"
        throw
    }
}

function Get-ReleaseByVersion {
    param([string]$Version)
    
    Write-ColorOutput "Fetching release information for version $Version..." "Info"
    
    try {
        $uri = "https://api.github.com/repos/$RepoOwner/$RepoName/releases/tags/$Version"
        $response = Invoke-RestMethod -Uri $uri -Headers @{"Accept" = "application/vnd.github.v3+json"}
        return $response
    }
    catch {
        Write-ColorOutput "Failed to fetch release information for version $Version: $($_.Exception.Message)" "Error"
        throw
    }
}

function Get-WindowsAsset {
    param($Release)
    
    # Look for Windows assets in priority order
    $preferredAssets = @(
        "*win-x64.msix",
        "*win-x64.zip",
        "*windows*.zip",
        "*win*.zip"
    )
    
    foreach ($pattern in $preferredAssets) {
        $asset = $Release.assets | Where-Object { $_.name -like $pattern } | Select-Object -First 1
        if ($asset) {
            return $asset
        }
    }
    
    throw "No suitable Windows asset found in release"
}

function Download-Asset {
    param($Asset, [string]$DownloadPath)
    
    Write-ColorOutput "Downloading $($Asset.name) ($('{0:N2}' -f ($Asset.size / 1MB)) MB)..." "Info"
    
    try {
        # Create download directory if it doesn't exist
        $downloadDir = Split-Path $DownloadPath -Parent
        if (-not (Test-Path $downloadDir)) {
            New-Item -ItemType Directory -Path $downloadDir -Force | Out-Null
        }
        
        # Download with progress
        $webClient = New-Object System.Net.WebClient
        $webClient.DownloadFile($Asset.browser_download_url, $DownloadPath)
        $webClient.Dispose()
        
        Write-ColorOutput "Download completed: $DownloadPath" "Success"
        return $true
    }
    catch {
        Write-ColorOutput "Download failed: $($_.Exception.Message)" "Error"
        return $false
    }
}

function Install-MSIXPackage {
    param([string]$PackagePath)
    
    Write-ColorOutput "Installing MSIX package..." "Info"
    
    try {
        Add-AppxPackage -Path $PackagePath -ForceApplicationShutdown
        Write-ColorOutput "MSIX package installed successfully!" "Success"
        return $true
    }
    catch {
        Write-ColorOutput "MSIX installation failed: $($_.Exception.Message)" "Warning"
        Write-ColorOutput "Falling back to manual installation..." "Info"
        return $false
    }
}

function Install-ZipPackage {
    param([string]$ZipPath, [string]$InstallPath)
    
    Write-ColorOutput "Extracting application to $InstallPath..." "Info"
    
    try {
        # Remove existing installation if Force is specified
        if ($Force -and (Test-Path $InstallPath)) {
            Write-ColorOutput "Removing existing installation..." "Warning"
            Remove-Item -Path $InstallPath -Recurse -Force
        }
        
        # Create install directory
        if (-not (Test-Path $InstallPath)) {
            New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
        }
        
        # Extract archive
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [System.IO.Compression.ZipFile]::ExtractToDirectory($ZipPath, $InstallPath)
        
        Write-ColorOutput "Application extracted successfully!" "Success"
        return $true
    }
    catch {
        Write-ColorOutput "Extraction failed: $($_.Exception.Message)" "Error"
        return $false
    }
}

function Create-StartMenuShortcut {
    param([string]$InstallPath)
    
    $exePath = Join-Path $InstallPath $ExeName
    if (-not (Test-Path $exePath)) {
        # Look for the executable in subdirectories
        $exePath = Get-ChildItem -Path $InstallPath -Name $ExeName -Recurse | Select-Object -First 1
        if ($exePath) {
            $exePath = Join-Path $InstallPath $exePath
        } else {
            Write-ColorOutput "Executable not found, skipping shortcut creation" "Warning"
            return
        }
    }
    
    try {
        $startMenuPath = [Environment]::GetFolderPath("StartMenu")
        $shortcutPath = Join-Path $startMenuPath "Programs\$AppName.lnk"
        
        $WScriptShell = New-Object -ComObject WScript.Shell
        $shortcut = $WScriptShell.CreateShortcut($shortcutPath)
        $shortcut.TargetPath = $exePath
        $shortcut.WorkingDirectory = $InstallPath
        $shortcut.Description = "GitHub PR Review Tool - Streamline your code reviews"
        $shortcut.Save()
        
        Write-ColorOutput "Start menu shortcut created: $shortcutPath" "Success"
    }
    catch {
        Write-ColorOutput "Failed to create start menu shortcut: $($_.Exception.Message)" "Warning"
    }
}

function Add-ToPath {
    param([string]$InstallPath)
    
    try {
        $currentPath = [Environment]::GetEnvironmentVariable("Path", "User")
        if ($currentPath -notlike "*$InstallPath*") {
            $newPath = "$currentPath;$InstallPath"
            [Environment]::SetEnvironmentVariable("Path", $newPath, "User")
            Write-ColorOutput "Added to user PATH: $InstallPath" "Success"
        }
    }
    catch {
        Write-ColorOutput "Failed to add to PATH: $($_.Exception.Message)" "Warning"
    }
}

function Show-CompletionMessage {
    param([string]$InstallPath, [bool]$MSIXInstalled)
    
    Write-ColorOutput "`n$AppName has been installed successfully!" "Success"
    
    if ($MSIXInstalled) {
        Write-ColorOutput "The application has been installed as an MSIX package and should appear in your Start menu." "Info"
    } else {
        Write-ColorOutput "Installation directory: $InstallPath" "Info"
        Write-ColorOutput "You can start the application from the Start menu or by running:" "Info"
        Write-ColorOutput "  GitHubPrTool.Desktop.exe" "Info"
    }
    
    Write-ColorOutput "`nFor help and documentation, visit:" "Info"
    Write-ColorOutput "  https://github.com/$RepoOwner/$RepoName" "Info"
}

# Main installation logic
try {
    Write-ColorOutput "GitHub PR Review Tool - Installation Script" "Success"
    Write-ColorOutput "==========================================" "Success"
    
    # Check PowerShell version
    if ($PSVersionTable.PSVersion.Major -lt 5) {
        throw "PowerShell 5.0 or later is required"
    }
    
    # Get release information
    if ($Version -eq "latest") {
        $release = Get-LatestRelease
    } else {
        $release = Get-ReleaseByVersion $Version
    }
    
    Write-ColorOutput "Installing version: $($release.tag_name)" "Info"
    
    # Find suitable Windows asset
    $asset = Get-WindowsAsset $release
    Write-ColorOutput "Selected asset: $($asset.name)" "Info"
    
    # Download asset
    $downloadPath = Join-Path $env:TEMP $asset.name
    if (-not (Download-Asset $asset $downloadPath)) {
        throw "Failed to download asset"
    }
    
    # Install based on file type
    $msixInstalled = $false
    if ($asset.name -like "*.msix") {
        $msixInstalled = Install-MSIXPackage $downloadPath
    }
    
    if (-not $msixInstalled) {
        # Manual installation from ZIP
        if (-not (Install-ZipPackage $downloadPath $InstallPath)) {
            throw "Failed to install application"
        }
        
        # Create shortcuts and add to PATH
        Create-StartMenuShortcut $InstallPath
        Add-ToPath $InstallPath
    }
    
    # Cleanup
    if (Test-Path $downloadPath) {
        Remove-Item $downloadPath -Force
    }
    
    # Show completion message
    Show-CompletionMessage $InstallPath $msixInstalled
    
    Write-ColorOutput "`nInstallation completed successfully!" "Success"
}
catch {
    Write-ColorOutput "`nInstallation failed: $($_.Exception.Message)" "Error"
    exit 1
}