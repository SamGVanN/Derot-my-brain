# Derot My Brain - Desktop Deployment Guide

This guide explains how to build and deploy **Derot My Brain** as a native desktop application using Tauri.

## Overview

Derot My Brain is now packaged as a native desktop application using [Tauri](https://tauri.app/). Tauri provides a lightweight native shell that:
- Launches the ASP.NET Core backend automatically
- Opens a native window with embedded WebView
- Manages the application lifecycle
- Bundles everything into platform-specific installers

## Prerequisites

### Development Machine Requirements

1. **.NET 9 SDK** - For building the ASP.NET Core backend
   ```powershell
   dotnet --version  # Should be 9.0 or higher
   ```

2. **Node.js & npm** - For building the React frontend and Tauri CLI
   ```powershell
   node --version  # Should be 18.0 or higher
   npm --version
   ```

3. **Rust** - Required for Tauri compilation
   ```powershell
   rustc --version  # Should be 1.70 or higher
   cargo --version
   ```
   
   **Install Rust**: https://www.rust-lang.org/tools/install

4. **Platform-Specific Tools**:
   - **Windows**: No additional requirements
   - **macOS**: Xcode Command Line Tools
   - **Linux**: Various build dependencies (see Tauri docs)

## Building the Application

### Quick Build (Windows Only)

```powershell
cd d:\Repos\Derot-my-brain\scripts
.\build-tauri.ps1 -WindowsOnly
```

### Full Build (All Platforms)

```powershell
cd d:\Repos\Derot-my-brain\scripts
.\build-tauri.ps1
```

### Build Options

```powershell
# Skip frontend build (if already built)
.\build-tauri.ps1 -SkipFrontend

# Skip backend build (if already built)
.\build-tauri.ps1 -SkipBackend

# Build only for Windows
.\build-tauri.ps1 -WindowsOnly

# Debug build
.\build-tauri.ps1 -Configuration Debug
```

## Build Output

After a successful build, installers are located in:

```
d:\Repos\Derot-my-brain\src-tauri\target\release\bundle\
```

### Windows
- **MSI Installer**: `msi\Derot My Brain_0.1.0_x64_en-US.msi`
- **NSIS Installer**: `nsis\Derot My Brain_0.1.0_x64-setup.exe`

### macOS
- **DMG**: `dmg\Derot My Brain_0.1.0_x64.dmg`
- **App Bundle**: `macos\Derot My Brain.app`

### Linux
- **Debian Package**: `deb\derot-my-brain_0.1.0_amd64.deb`
- **AppImage**: `appimage\derot-my-brain_0.1.0_amd64.AppImage`

## Installation

### End-User Installation (Windows)

1. **Download** the installer (`.msi` or `.exe`)
2. **Run** the installer
3. **Launch** from Start Menu: `Derot My Brain`

The application will:
- Start the backend automatically
- Open in a native window
- Run independently of the browser
- Appear in the system tray (if configured)

### Uninstallation

**Windows**: Use "Add or Remove Programs" in Settings

**macOS**: Drag the app to Trash

**Linux**: 
- Debian: `sudo apt remove derot-my-brain`
- AppImage: Delete the `.AppImage` file

## Development Workflow

### Testing Locally (Tauri Dev Mode)

```powershell
# Terminal 1: Start the backend
cd src\backend\DerotMyBrain.API
dotnet run

# Terminal 2: Start Tauri dev mode
cd d:\Repos\Derot-my-brain
npm run tauri:dev
```

This opens a native window pointing to your dev server.

### Debugging

- **Backend Logs**: `Logs/log-YYYY-MM-DD.txt`
- **Tauri Logs**: Check console output during development
- **Frontend**: Use browser DevTools (F12 in Tauri window)

## Distribution

### Code Signing (Recommended for Production)

For production distribution, you should sign your installers:

**Windows**: 
- Requires a code signing certificate
- Update `tauri.conf.json` with certificate details

**macOS**:
- Requires Apple Developer account
- Configure app signing and notarization

**Linux**:
- AppImage: Optional but recommended
- Debian: Use GPG key for repository

See [Tauri Code Signing Guide](https://tauri.app/v1/guides/distribution/sign-your-application)

### Auto-Updates (Optional)

Tauri supports automatic updates. See:
- [Tauri Updater Plugin](https://tauri.app/v1/guides/distribution/updater)

## Architecture

```
Desktop Application (Tauri)
├── Native Shell (Rust)
│   ├── Launches DerotMyBrain.exe
│   ├── Manages process lifecycle
│   └── Opens WebView window
│
├── Backend (ASP.NET Core)
│   ├── Runs on http://127.0.0.1:5173
│   ├── Serves API endpoints
│   └── Serves static frontend files
│
└── Frontend (React SPA)
    └── Loaded in WebView from backend
```

## Troubleshooting

### "Backend not found" error

The backend executable should be bundled automatically. Check:
1. `published-backend/win-x64/` contains `DerotMyBrain.exe`
2. `src-tauri/tauri.conf.json` has correct resources paths
3. Rebuild with `.\build-tauri.ps1`

### "Connection refused" error

The backend didn't start. Check:
1. Backend executable is included in bundle
2. Firewall isn't blocking port 5173
3. No other application is using port 5173

### Build fails with Rust errors

1. Ensure Rust is installed: `rustc --version`
2. Update Rust: `rustup update`
3. Clear build cache: `cd src-tauri && cargo clean`

### MSI installer shows security warning

This is expected for unsigned installers. Options:
1. Click "More info" → "Run anyway"
2. Obtain a code signing certificate (for production)

## CI/CD (Optional)

For automated builds on GitHub Actions, see:
- [Tauri GitHub Actions Guide](https://tauri.app/v1/guides/building/cross-platform)

## Migration from Standalone

If you were using the old standalone deployment:
- The `publish.ps1` and `publish.sh` scripts have been removed
- SystemTrayService is no longer used (Tauri handles native shell)
- Auto-browser launch is no longer needed (Tauri opens WebView)
- All functionality is now integrated into Tauri

## Support

For issues specific to:
- **Tauri**: https://github.com/tauri-apps/tauri/issues
- **Derot My Brain**: Check project documentation
