#!/bin/bash

# GitHub PR Review Tool - Installation Script for Unix-like systems
# This script downloads and installs the latest version of GitHub PR Review Tool

set -e

# Configuration
REPO_OWNER="nam20485"
REPO_NAME="pr-batch-comment-tool"
APP_NAME="GitHub PR Review Tool"
INSTALL_DIR="$HOME/.local/share/GitHubPrTool"
BIN_DIR="$HOME/.local/bin"
DESKTOP_DIR="$HOME/.local/share/applications"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Default values
VERSION="latest"
FORCE=false
QUIET=false

# Usage information
usage() {
    cat << EOF
GitHub PR Review Tool Installation Script

Usage: $0 [OPTIONS]

Options:
    -v, --version VERSION    Install specific version (default: latest)
    -d, --dir DIRECTORY      Installation directory (default: ~/.local/share/GitHubPrTool)
    -f, --force              Force reinstallation
    -q, --quiet              Quiet mode (minimal output)
    -h, --help               Show this help message

Examples:
    $0                       # Install latest version
    $0 -v v1.2.0            # Install specific version
    $0 -f                   # Force reinstall
    $0 -d /opt/GitHubPrTool # Install to custom directory

EOF
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -v|--version)
            VERSION="$2"
            shift 2
            ;;
        -d|--dir)
            INSTALL_DIR="$2"
            shift 2
            ;;
        -f|--force)
            FORCE=true
            shift
            ;;
        -q|--quiet)
            QUIET=true
            shift
            ;;
        -h|--help)
            usage
            exit 0
            ;;
        *)
            echo "Unknown option: $1" >&2
            usage >&2
            exit 1
            ;;
    esac
done

# Logging functions
log() {
    if [ "$QUIET" != true ]; then
        echo -e "${GREEN}[INFO]${NC} $1"
    fi
}

warn() {
    if [ "$QUIET" != true ]; then
        echo -e "${YELLOW}[WARN]${NC} $1" >&2
    fi
}

error() {
    echo -e "${RED}[ERROR]${NC} $1" >&2
}

success() {
    if [ "$QUIET" != true ]; then
        echo -e "${GREEN}[SUCCESS]${NC} $1"
    fi
}

# Check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Detect system information
detect_system() {
    local os=$(uname -s | tr '[:upper:]' '[:lower:]')
    local arch=$(uname -m)
    
    case "$os" in
        linux*)
            PLATFORM="linux"
            ;;
        darwin*)
            PLATFORM="osx"
            ;;
        *)
            error "Unsupported operating system: $os"
            exit 1
            ;;
    esac
    
    case "$arch" in
        x86_64|amd64)
            ARCH="x64"
            ;;
        arm64|aarch64)
            ARCH="arm64"
            ;;
        *)
            warn "Unsupported architecture: $arch, trying x64"
            ARCH="x64"
            ;;
    esac
    
    RUNTIME="${PLATFORM}-${ARCH}"
    log "Detected platform: $RUNTIME"
}

# Check prerequisites
check_prerequisites() {
    log "Checking prerequisites..."
    
    # Check for required commands
    local missing_commands=()
    
    if ! command_exists curl && ! command_exists wget; then
        missing_commands+=("curl or wget")
    fi
    
    if ! command_exists tar; then
        missing_commands+=("tar")
    fi
    
    if ! command_exists jq; then
        missing_commands+=("jq")
    fi
    
    if [ ${#missing_commands[@]} -ne 0 ]; then
        error "Missing required commands: ${missing_commands[*]}"
        echo "Please install them using your package manager:"
        case "$PLATFORM" in
            linux)
                echo "  Ubuntu/Debian: sudo apt update && sudo apt install curl tar jq"
                echo "  CentOS/RHEL: sudo yum install curl tar jq"
                echo "  Arch: sudo pacman -S curl tar jq"
                ;;
            osx)
                echo "  Homebrew: brew install curl tar jq"
                echo "  MacPorts: sudo port install curl tar jq"
                ;;
        esac
        exit 1
    fi
    
    success "Prerequisites check passed"
}

# Download function that tries curl first, then wget
download() {
    local url="$1"
    local output="$2"
    
    if command_exists curl; then
        curl -fsSL "$url" -o "$output"
    elif command_exists wget; then
        wget -q "$url" -O "$output"
    else
        error "No download tool available (curl or wget required)"
        exit 1
    fi
}

# Get latest release information
get_latest_release() {
    log "Fetching latest release information..."
    
    local api_url="https://api.github.com/repos/$REPO_OWNER/$REPO_NAME/releases/latest"
    local temp_file=$(mktemp)
    
    if ! download "$api_url" "$temp_file"; then
        error "Failed to fetch release information"
        rm -f "$temp_file"
        exit 1
    fi
    
    RELEASE_TAG=$(jq -r '.tag_name' "$temp_file")
    RELEASE_NAME=$(jq -r '.name' "$temp_file")
    
    rm -f "$temp_file"
    
    if [ "$RELEASE_TAG" == "null" ]; then
        error "Failed to parse release information"
        exit 1
    fi
    
    log "Latest release: $RELEASE_TAG"
}

# Get specific release information
get_release_by_version() {
    local version="$1"
    log "Fetching release information for version $version..."
    
    local api_url="https://api.github.com/repos/$REPO_OWNER/$REPO_NAME/releases/tags/$version"
    local temp_file=$(mktemp)
    
    if ! download "$api_url" "$temp_file"; then
        error "Failed to fetch release information for version $version"
        rm -f "$temp_file"
        exit 1
    fi
    
    RELEASE_TAG=$(jq -r '.tag_name' "$temp_file")
    RELEASE_NAME=$(jq -r '.name' "$temp_file")
    
    rm -f "$temp_file"
    
    if [ "$RELEASE_TAG" == "null" ]; then
        error "Version $version not found"
        exit 1
    fi
}

# Find suitable asset for the platform
find_platform_asset() {
    log "Finding suitable asset for $RUNTIME..."
    
    local api_url="https://api.github.com/repos/$REPO_OWNER/$REPO_NAME/releases/tags/$RELEASE_TAG"
    local temp_file=$(mktemp)
    
    if ! download "$api_url" "$temp_file"; then
        error "Failed to fetch release assets"
        rm -f "$temp_file"
        exit 1
    fi
    
    # Try to find platform-specific asset
    local patterns=(
        ".*$RUNTIME\\.tar\\.gz"
        ".*$PLATFORM.*\\.tar\\.gz"
        ".*\\.tar\\.gz"
    )
    
    for pattern in "${patterns[@]}"; do
        ASSET_URL=$(jq -r ".assets[] | select(.name | test(\"$pattern\")) | .browser_download_url" "$temp_file" | head -n1)
        ASSET_NAME=$(jq -r ".assets[] | select(.name | test(\"$pattern\")) | .name" "$temp_file" | head -n1)
        
        if [ "$ASSET_URL" != "" ] && [ "$ASSET_URL" != "null" ]; then
            break
        fi
    done
    
    rm -f "$temp_file"
    
    if [ "$ASSET_URL" == "" ] || [ "$ASSET_URL" == "null" ]; then
        error "No suitable asset found for platform $RUNTIME"
        exit 1
    fi
    
    log "Selected asset: $ASSET_NAME"
}

# Download and extract application
download_and_extract() {
    log "Downloading $ASSET_NAME..."
    
    local temp_dir=$(mktemp -d)
    local download_path="$temp_dir/$ASSET_NAME"
    
    if ! download "$ASSET_URL" "$download_path"; then
        error "Failed to download asset"
        rm -rf "$temp_dir"
        exit 1
    fi
    
    success "Download completed"
    
    # Remove existing installation if force flag is set
    if [ "$FORCE" == true ] && [ -d "$INSTALL_DIR" ]; then
        warn "Removing existing installation..."
        rm -rf "$INSTALL_DIR"
    fi
    
    # Create installation directory
    mkdir -p "$INSTALL_DIR"
    
    log "Extracting application to $INSTALL_DIR..."
    
    if ! tar -xzf "$download_path" -C "$INSTALL_DIR" --strip-components=1; then
        error "Failed to extract application"
        rm -rf "$temp_dir"
        exit 1
    fi
    
    # Cleanup
    rm -rf "$temp_dir"
    
    success "Application extracted successfully"
}

# Make executable and create symlink
setup_executable() {
    log "Setting up executable..."
    
    # Find the main executable
    local exe_name="GitHubPrTool.Desktop"
    local exe_path="$INSTALL_DIR/$exe_name"
    
    if [ ! -f "$exe_path" ]; then
        # Try to find executable in subdirectories
        exe_path=$(find "$INSTALL_DIR" -name "$exe_name" -type f | head -n1)
        
        if [ ! -f "$exe_path" ]; then
            error "Executable not found: $exe_name"
            exit 1
        fi
    fi
    
    # Make executable
    chmod +x "$exe_path"
    
    # Create bin directory and symlink
    mkdir -p "$BIN_DIR"
    local symlink_path="$BIN_DIR/githubprtool"
    
    if [ -L "$symlink_path" ] || [ -f "$symlink_path" ]; then
        rm -f "$symlink_path"
    fi
    
    ln -s "$exe_path" "$symlink_path"
    
    success "Executable setup completed"
}

# Create desktop entry (Linux only)
create_desktop_entry() {
    if [ "$PLATFORM" != "linux" ]; then
        return
    fi
    
    log "Creating desktop entry..."
    
    mkdir -p "$DESKTOP_DIR"
    
    cat > "$DESKTOP_DIR/githubprtool.desktop" << EOF
[Desktop Entry]
Name=GitHub PR Review Tool
Comment=Streamline GitHub Pull Request reviews with batch comment operations
Exec=$BIN_DIR/githubprtool
Icon=$INSTALL_DIR/assets/githubprtool.png
Terminal=false
Type=Application
Categories=Development;
StartupNotify=true
EOF
    
    success "Desktop entry created"
}

# Update PATH if needed
update_path() {
    local shell_rc=""
    
    # Determine shell configuration file
    case "$SHELL" in
        */bash)
            if [ -f "$HOME/.bashrc" ]; then
                shell_rc="$HOME/.bashrc"
            elif [ -f "$HOME/.bash_profile" ]; then
                shell_rc="$HOME/.bash_profile"
            fi
            ;;
        */zsh)
            shell_rc="$HOME/.zshrc"
            ;;
        */fish)
            shell_rc="$HOME/.config/fish/config.fish"
            ;;
    esac
    
    # Check if BIN_DIR is in PATH
    if [[ ":$PATH:" != *":$BIN_DIR:"* ]]; then
        if [ -n "$shell_rc" ] && [ -f "$shell_rc" ]; then
            echo "" >> "$shell_rc"
            echo "# Added by GitHub PR Review Tool installer" >> "$shell_rc"
            echo "export PATH=\"\$PATH:$BIN_DIR\"" >> "$shell_rc"
            
            warn "Added $BIN_DIR to PATH in $shell_rc"
            warn "Run 'source $shell_rc' or restart your terminal to use 'githubprtool' command"
        else
            warn "Could not automatically update PATH"
            warn "Add $BIN_DIR to your PATH manually"
        fi
    fi
}

# Show completion message
show_completion() {
    echo
    success "$APP_NAME has been installed successfully!"
    echo
    log "Installation directory: $INSTALL_DIR"
    log "Executable: $BIN_DIR/githubprtool"
    echo
    log "You can start the application by running:"
    log "  githubprtool"
    echo
    log "For help and documentation, visit:"
    log "  https://github.com/$REPO_OWNER/$REPO_NAME"
    echo
}

# Main installation logic
main() {
    echo -e "${GREEN}GitHub PR Review Tool - Installation Script${NC}"
    echo "============================================="
    echo
    
    # System detection and prerequisites
    detect_system
    check_prerequisites
    
    # Get release information
    if [ "$VERSION" == "latest" ]; then
        get_latest_release
    else
        get_release_by_version "$VERSION"
    fi
    
    log "Installing version: $RELEASE_TAG"
    
    # Find and download asset
    find_platform_asset
    download_and_extract
    
    # Setup application
    setup_executable
    create_desktop_entry
    update_path
    
    # Show completion message
    show_completion
    
    success "Installation completed successfully!"
}

# Error handling
trap 'error "Installation failed"; exit 1' ERR

# Run main function
main "$@"