# GitHub PR Review Tool - Build and Package Script
# This script builds the application for multiple platforms and creates packages

param(
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0",
    [string[]]$Platforms = @("win-x64", "osx-x64", "linux-x64"),
    [string]$OutputDir = "dist",
    [switch]$SkipTests,
    [switch]$CreatePackages,
    [switch]$SignPackages,
    [string]$CertificateThumbprint = "",
    [switch]$Clean
)

$ErrorActionPreference = "Stop"

# Project paths
$SolutionFile = "GitHubPrTool.sln"
$DesktopProject = "src/GitHubPrTool.Desktop/GitHubPrTool.Desktop.csproj"
$TestProject = "tests"

# Output configuration
$BuildOutputDir = Join-Path $OutputDir "build"
$PackageOutputDir = Join-Path $OutputDir "packages"

function Write-BuildLog {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $color = switch ($Level) {
        "ERROR" { "Red" }
        "WARN" { "Yellow" }
        "SUCCESS" { "Green" }
        default { "White" }
    }
    Write-Host "[$timestamp] [$Level] $Message" -ForegroundColor $color
}

function Test-Prerequisites {
    Write-BuildLog "Checking prerequisites..."
    
    # Check .NET SDK
    try {
        $dotnetVersion = dotnet --version
        Write-BuildLog ".NET SDK version: $dotnetVersion" "SUCCESS"
    }
    catch {
        Write-BuildLog ".NET SDK not found. Please install .NET 8.0 SDK or later." "ERROR"
        throw
    }
    
    # Check solution file
    if (-not (Test-Path $SolutionFile)) {
        Write-BuildLog "Solution file not found: $SolutionFile" "ERROR"
        throw
    }
    
    # Check for signing certificate if required
    if ($SignPackages -and -not $CertificateThumbprint) {
        Write-BuildLog "Certificate thumbprint required for signing" "ERROR"
        throw
    }
    
    Write-BuildLog "Prerequisites check passed" "SUCCESS"
}

function Clear-OutputDirectories {
    if ($Clean) {
        Write-BuildLog "Cleaning output directories..."
        
        if (Test-Path $OutputDir) {
            Remove-Item -Path $OutputDir -Recurse -Force
        }
        
        # Clean bin and obj directories
        Get-ChildItem -Path . -Include "bin", "obj" -Recurse -Directory | Remove-Item -Recurse -Force
        
        Write-BuildLog "Clean completed" "SUCCESS"
    }
    
    # Ensure output directories exist
    New-Item -ItemType Directory -Path $BuildOutputDir -Force | Out-Null
    New-Item -ItemType Directory -Path $PackageOutputDir -Force | Out-Null
}

function Restore-Dependencies {
    Write-BuildLog "Restoring NuGet packages..."
    
    try {
        dotnet restore $SolutionFile --verbosity minimal
        Write-BuildLog "Package restore completed" "SUCCESS"
    }
    catch {
        Write-BuildLog "Package restore failed" "ERROR"
        throw
    }
}

function Run-Tests {
    if ($SkipTests) {
        Write-BuildLog "Skipping tests (SkipTests flag set)" "WARN"
        return
    }
    
    Write-BuildLog "Running tests..."
    
    try {
        $testResult = dotnet test $SolutionFile --configuration $Configuration --logger "console;verbosity=minimal" --collect:"XPlat Code Coverage"
        
        if ($LASTEXITCODE -eq 0) {
            Write-BuildLog "All tests passed" "SUCCESS"
        } else {
            Write-BuildLog "Some tests failed" "ERROR"
            throw "Test execution failed"
        }
    }
    catch {
        Write-BuildLog "Test execution failed: $($_.Exception.Message)" "ERROR"
        throw
    }
}

function Build-ForPlatform {
    param([string]$Platform)
    
    Write-BuildLog "Building for platform: $Platform"
    
    $platformOutputDir = Join-Path $BuildOutputDir $Platform
    New-Item -ItemType Directory -Path $platformOutputDir -Force | Out-Null
    
    try {
        $publishArgs = @(
            "publish", $DesktopProject,
            "--configuration", $Configuration,
            "--runtime", $Platform,
            "--self-contained", "true",
            "--output", $platformOutputDir,
            "/p:Version=$Version",
            "/p:AssemblyVersion=$Version.0",
            "/p:FileVersion=$Version.0",
            "/p:PublishSingleFile=true",
            "/p:PublishTrimmed=false",
            "--verbosity", "minimal"
        )
        
        # Add platform-specific arguments
        if ($Platform -eq "win-x64") {
            $publishArgs += "/p:WindowsPackageType=None"  # Skip MSIX for regular build
        }
        
        dotnet @publishArgs
        
        if ($LASTEXITCODE -eq 0) {
            Write-BuildLog "Build completed for $Platform" "SUCCESS"
        } else {
            throw "Build failed for $Platform"
        }
    }
    catch {
        Write-BuildLog "Build failed for platform $Platform : $($_.Exception.Message)" "ERROR"
        throw
    }
}

function Create-ZipPackage {
    param([string]$Platform, [string]$SourceDir)
    
    $packageName = "GitHubPrTool-$Version-$Platform.zip"
    $packagePath = Join-Path $PackageOutputDir $packageName
    
    Write-BuildLog "Creating ZIP package: $packageName"
    
    try {
        Compress-Archive -Path (Join-Path $SourceDir '*') -DestinationPath $packagePath -Force
        
        $packageSize = (Get-Item $packagePath).Length / 1MB
        Write-BuildLog "ZIP package created: $packageName ($('{0:N2}' -f $packageSize) MB)" "SUCCESS"
        
        return $packagePath
    }
    catch {
        Write-BuildLog "Failed to create ZIP package: $($_.Exception.Message)" "ERROR"
        throw
    }
}

function Create-MSIXPackage {
    param([string]$SourceDir)
    
    Write-BuildLog "Creating MSIX package..."
    
    try {
        # Build MSIX package using dotnet publish
        $msixOutputDir = Join-Path $PackageOutputDir "msix"
        New-Item -ItemType Directory -Path $msixOutputDir -Force | Out-Null
        
        $publishArgs = @(
            "publish", $DesktopProject,
            "--configuration", $Configuration,
            "--runtime", "win-x64",
            "--self-contained", "true",
            "--output", $msixOutputDir,
            "/p:Version=$Version",
            "/p:WindowsPackageType=MSIX",
            "/p:WindowsAppSDKSelfContained=true",
            "/p:GenerateAppInstallerFile=true",
            "/p:AppxPackageDir=$PackageOutputDir/",
            "--verbosity", "minimal"
        )
        
        dotnet @publishArgs
        
        # Find generated MSIX file
        $msixFile = Get-ChildItem -Path $PackageOutputDir -Filter "*.msix" | Select-Object -First 1
        
        if ($msixFile) {
            $packageSize = $msixFile.Length / 1MB
            Write-BuildLog "MSIX package created: $($msixFile.Name) ($('{0:N2}' -f $packageSize) MB)" "SUCCESS"
            return $msixFile.FullName
        } else {
            Write-BuildLog "MSIX package not found after build" "ERROR"
            throw "MSIX package creation failed"
        }
    }
    catch {
        Write-BuildLog "MSIX package creation failed: $($_.Exception.Message)" "ERROR"
        throw
    }
}

function Sign-Package {
    param([string]$PackagePath)
    
    if (-not $SignPackages) {
        return
    }
    
    Write-BuildLog "Signing package: $(Split-Path $PackagePath -Leaf)"
    
    try {
        # Sign the package using SignTool
        $signTool = "${env:ProgramFiles(x86)}\Windows Kits\10\bin\x64\signtool.exe"
        
        if (-not (Test-Path $signTool)) {
            Write-BuildLog "SignTool not found. Please install Windows SDK." "WARN"
            return
        }
        
        $signArgs = @(
            "sign",
            "/sha1", $CertificateThumbprint,
            "/t", "http://timestamp.digicert.com",
            "/fd", "SHA256",
            "`"$PackagePath`""
        )
        
        & $signTool @signArgs
        
        if ($LASTEXITCODE -eq 0) {
            Write-BuildLog "Package signed successfully" "SUCCESS"
        } else {
            Write-BuildLog "Package signing failed" "WARN"
        }
    }
    catch {
        Write-BuildLog "Package signing failed: $($_.Exception.Message)" "WARN"
    }
}

function Generate-ReleaseNotes {
    $releaseNotesPath = Join-Path $OutputDir "RELEASE_NOTES.md"
    
    $releaseNotes = @"
# GitHub PR Review Tool v$Version

## Release Information
- **Version**: $Version
- **Build Date**: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss UTC")
- **Configuration**: $Configuration
- **Platforms**: $($Platforms -join ", ")

## Packages

"@
    
    # Add package information
    $packages = Get-ChildItem -Path $PackageOutputDir -File
    foreach ($package in $packages) {
        $packageSize = $package.Length / 1MB
        $releaseNotes += "- **$($package.Name)** ($('{0:N2}' -f $packageSize) MB)`n"
    }
    
    $releaseNotes += @"

## Installation

### Windows
- **MSIX**: Double-click the .msix file to install
- **ZIP**: Extract to desired location and run GitHubPrTool.Desktop.exe
- **PowerShell**: ``irm https://raw.githubusercontent.com/nam20485/pr-batch-comment-tool/main/scripts/install.ps1 | iex``

### macOS
- **DMG**: Open the .dmg file and drag to Applications
- **TAR.GZ**: Extract and run the application

### Linux
- **AppImage**: Make executable and run
- **TAR.GZ**: Extract and run the application

## System Requirements
- **.NET 8.0 Runtime** (included in self-contained packages)
- **Windows 10/11**, **macOS 10.15+**, or **Linux** with X11/Wayland
- **Internet connection** for GitHub API access
- **2GB RAM** minimum, 4GB recommended

## What's New
- Refer to CHANGELOG.md for detailed release notes

## Support
- **Issues**: https://github.com/nam20485/pr-batch-comment-tool/issues
- **Documentation**: https://github.com/nam20485/pr-batch-comment-tool/wiki
- **Discussions**: https://github.com/nam20485/pr-batch-comment-tool/discussions

---
Built with ❤️ for the GitHub developer community
"@
    
    Set-Content -Path $releaseNotesPath -Value $releaseNotes -Encoding UTF8
    Write-BuildLog "Release notes generated: $releaseNotesPath" "SUCCESS"
}

# Main build process
try {
    Write-BuildLog "Starting build process for GitHub PR Review Tool v$Version" "SUCCESS"
    Write-BuildLog "Configuration: $Configuration, Platforms: $($Platforms -join ', ')"
    
    # Prerequisites and setup
    Test-Prerequisites
    Clear-OutputDirectories
    Restore-Dependencies
    
    # Run tests
    Run-Tests
    
    # Build for each platform
    $builtPlatforms = @()
    foreach ($platform in $Platforms) {
        try {
            Build-ForPlatform $platform
            $builtPlatforms += $platform
        }
        catch {
            Write-BuildLog "Failed to build for platform: $platform" "ERROR"
            if ($Platforms.Count -eq 1) {
                throw
            }
        }
    }
    
    # Create packages if requested
    if ($CreatePackages) {
        Write-BuildLog "Creating distribution packages..."
        
        foreach ($platform in $builtPlatforms) {
            $sourceDir = Join-Path $BuildOutputDir $platform
            
            if (Test-Path $sourceDir) {
                # Create ZIP package for all platforms
                $zipPackage = Create-ZipPackage $platform $sourceDir
                Sign-Package $zipPackage
                
                # Create MSIX package for Windows
                if ($platform -eq "win-x64") {
                    try {
                        $msixPackage = Create-MSIXPackage $sourceDir
                        Sign-Package $msixPackage
                    }
                    catch {
                        Write-BuildLog "MSIX package creation failed, but ZIP package is available" "WARN"
                    }
                }
            }
        }
        
        Generate-ReleaseNotes
    }
    
    # Build summary
    Write-BuildLog "Build process completed successfully!" "SUCCESS"
    Write-BuildLog "Built platforms: $($builtPlatforms -join ', ')"
    Write-BuildLog "Output directory: $OutputDir"
    
    if ($CreatePackages) {
        $packageCount = (Get-ChildItem -Path $PackageOutputDir -File).Count
        Write-BuildLog "Created $packageCount package(s)" "SUCCESS"
    }
}
catch {
    Write-BuildLog "Build process failed: $($_.Exception.Message)" "ERROR"
    exit 1
}