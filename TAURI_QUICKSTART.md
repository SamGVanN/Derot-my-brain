# Tauri Desktop Integration - Quick Start Guide

## Prerequisites

Before building the Tauri desktop application, you need:

1. **Rust** - Download from https://www.rust-lang.org/tools/install
   - Run the installer
   - Restart your terminal after installation
   - Verify: `rustc --version`

2. **.NET 9 SDK** - Already installed ✓

3. **Node.js** - Already installed ✓

## How to Build the MSI Installer

### Quick Start (Recommended)

```powershell
cd d:\Repos\Derot-my-brain\scripts
.\build-tauri.ps1 --WindowsOnly
```

**That's it!** This single command will:
1. ✅ Build the React frontend → `dist/`
2. ✅ Publish the .NET backend → `published-backend/win-x64/`
3. ✅ Copy frontend to backend's `wwwroot/`
4. ✅ Build Tauri bundle with backend + frontend
5. ✅ Generate Windows installers (MSI + NSIS)

**Output Location**: `src-tauri\target\release\bundle\msi\Derot My Brain_0.1.0_x64_en-US.msi`

---

## Build Options

### Full Cross-Platform Build

```powershell
.\build-tauri.ps1
```
Builds for Windows, Linux, macOS Intel, and macOS ARM (slower, ~5-10 min)

### Windows-Only Build (Faster)

```powershell
.\build-tauri.ps1 --WindowsOnly
```
Builds only for Windows (~3-5 min)

### Incremental Builds

```powershell
# Skip frontend rebuild (use existing dist/)
.\build-tauri.ps1 --SkipFrontend

# Skip backend rebuild (use existing published-backend/)
.\build-tauri.ps1 --SkipBackend

# Skip both (just rebuild Tauri bundle)
.\build-tauri.ps1 --SkipFrontend --SkipBackend
```

---

## Installation & Testing

### Install the MSI

1. Navigate to:
   ```powershell
   cd src-tauri\target\release\bundle\msi
   ```

2. Double-click: `Derot My Brain_0.1.0_x64_en-US.msi`

3. Follow installation wizard

4. Launch from Start Menu or Desktop shortcut

### Verify Application Works

✅ **Application launches** (no console window)  
✅ **Backend starts automatically** on port 45123  
✅ **Login page loads** correctly  
✅ **All features work** (login, quiz, documents, etc.)  
✅ **Application closes** gracefully without errors

---

## Development Mode (No Installer)

For development without building the full installer:

```powershell
# Terminal 1: Start backend manually
cd src\backend\DerotMyBrain.API
dotnet run

# Terminal 2: Start Tauri in dev mode
cd d:\Repos\Derot-my-brain
npm run tauri:dev
```

**Note**: In dev mode, backend runs on port 45123 and Tauri opens a debug window.

---

## Architecture Overview

### Production Bundle Structure

```
Installed Application
├── Derot My Brain.exe (Tauri launcher)
└── DerotMyBrain.API.exe (Backend bundled inside)
    └── wwwroot/ (Frontend static files)
```

### How It Works

1. User launches `Derot My Brain.exe`
2. Tauri starts and launches `DerotMyBrain.API.exe`
3. Backend starts on `http://127.0.0.1:45123`
4. Frontend detects Tauri environment and points to `http://127.0.0.1:45123`
5. Tauri WebView loads frontend
6. Frontend calls API endpoints on `http://127.0.0.1:45123/api`
7. When app closes, Tauri kills backend process

**Result**: Self-contained desktop app with no external dependencies!

---

## Troubleshooting

### "Rust is not installed"
- Install: https://www.rust-lang.org/tools/install
- Restart PowerShell
- Verify: `rustc --version`

### "Cannot find DerotMyBrain.API.exe"
- Make sure you didn't skip backend build
- Check: `published-backend\win-x64\DerotMyBrain.API.exe` exists

### "Frontend build failed"
```powershell
cd src\frontend
npm install
npm run build
```

### "Port 45123 already in use"
- Another app is using port 45123 (rare)
- Stop it or change port in `Program.cs` and `client.ts`

### Windows SmartScreen Warning
- Click "More info" → "Run anyway"
- Expected for unsigned apps (need code signing certificate for production)

---

## Distribution

The `.msi` installer is **portable** and can be distributed:

1. ✅ Upload to GitHub Releases
2. ✅ Share directly with users
3. ✅ No dependencies required on user machines
4. ✅ Standard Windows installation experience

Users just download and run the `.msi` - no .NET, Node.js, or Rust needed!

---

## Cross-Platform Support

The same build script works on **all platforms**, but each platform must be built on its native OS.

### Windows 🪟

**On Windows (your current system):**

```powershell
cd d:\Repos\Derot-my-brain\scripts
.\build-tauri.ps1 --WindowsOnly
```

**Outputs**:
- ✅ `Derot My Brain_0.1.0_x64_en-US.msi` (MSI installer)
- ✅ `Derot My Brain_0.1.0_x64-setup.exe` (NSIS installer)

**Location**: `src-tauri\target\release\bundle\msi\` and `\nsis\`

---

### Linux 🐧

**On a Linux machine (Ubuntu/Debian recommended):**

#### Prerequisites
```bash
# Install Rust
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh

# Install system dependencies
sudo apt update
sudo apt install -y libgtk-3-dev libwebkit2gtk-4.0-dev \
    libappindicator3-dev librsvg2-dev patchelf

# Install .NET 9 SDK
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0

# Install Node.js (if not already installed)
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt install -y nodejs
```

#### Build
```bash
cd ~/Derot-my-brain/scripts
./build-tauri.ps1  # PowerShell works on Linux with pwsh
# OR if you don't have PowerShell:
chmod +x build-tauri.sh  # If you create a bash version
./build-tauri.sh
```

**Outputs**:
- ✅ `derot-my-brain_0.1.0_amd64.deb` (Debian/Ubuntu package)
- ✅ `derot-my-brain_0.1.0_amd64.AppImage` (Universal Linux app)

**Location**: `src-tauri/target/release/bundle/deb/` and `/appimage/`

---

### macOS 🍎

**On a macOS machine:**

#### Prerequisites
```bash
# Install Rust
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh

# Install .NET 9 SDK
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0

# Install Node.js (via Homebrew)
brew install node

# Xcode Command Line Tools (if not already installed)
xcode-select --install
```

#### Build (Intel)
```bash
cd ~/Derot-my-brain/scripts
./build-tauri.ps1
# This will build for your current architecture
```

**Outputs**:
- ✅ `Derot My Brain.app` (Mac application bundle)
- ✅ `Derot My Brain_0.1.0_x64.dmg` (Disk image installer)

**Location**: `src-tauri/target/release/bundle/macos/` and `/dmg/`

#### Build (Apple Silicon / ARM)
Same command, but on an Apple Silicon Mac it will produce:
- ✅ `Derot My Brain_0.1.0_aarch64.dmg`

---

### Building All Platforms from Windows

**Not directly possible** - each OS requires its native build tools. However, you have options:

#### Option 1: Virtual Machines
- Set up Ubuntu VM for Linux builds
- Set up macOS VM (requires Mac hardware for legal reasons)
- Run build script in each VM

#### Option 2: GitHub Actions (Recommended for CI/CD)

Create `.github/workflows/build-all-platforms.yml`:

```yaml
name: Build All Platforms

on:
  push:
    tags:
      - 'v*'

jobs:
  build-windows:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      - name: Build Windows
        run: .\scripts\build-tauri.ps1 --WindowsOnly
      - name: Upload MSI
        uses: actions/upload-artifact@v3
        with:
          name: windows-msi
          path: src-tauri/target/release/bundle/msi/*.msi

  build-linux:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Install dependencies
        run: |
          sudo apt update
          sudo apt install -y libgtk-3-dev libwebkit2gtk-4.0-dev
      - name: Build Linux
        run: ./scripts/build-tauri.ps1
      - name: Upload DEB
        uses: actions/upload-artifact@v3
        with:
          name: linux-deb
          path: src-tauri/target/release/bundle/deb/*.deb

  build-macos:
    runs-on: macos-latest
    steps:
      - uses: actions/checkout@v3
      - name: Build macOS
        run: ./scripts/build-tauri.ps1
      - name: Upload DMG
        uses: actions/upload-artifact@v3
        with:
          name: macos-dmg
          path: src-tauri/target/release/bundle/dmg/*.dmg
```

Then push a tag:
```bash
git tag v0.1.0
git push origin v0.1.0
```

GitHub Actions will automatically build for all platforms! 🎉

---

### Cross-Platform Build Summary

| Platform | Build On | Command | Output Format |
|----------|----------|---------|---------------|
| Windows | Windows | `.\build-tauri.ps1 --WindowsOnly` | `.msi`, `.exe` |
| Linux | Linux | `./build-tauri.ps1` | `.deb`, `.AppImage` |
| macOS Intel | macOS Intel | `./build-tauri.ps1` | `.dmg`, `.app` |
| macOS ARM | macOS ARM | `./build-tauri.ps1` | `.dmg` (aarch64), `.app` |

**All platforms** use the same `build-tauri.ps1` script with the same architecture fixes! ✅

---

## File Locations Reference

```
d:\Repos\Derot-my-brain\
├── dist/                              # Frontend build output
│   ├── assets/
│   └── index.html
│
├── published-backend/                 # Backend executables
│   ├── win-x64/
│   │   ├── DerotMyBrain.API.exe      # Backend binary
│   │   └── wwwroot/                   # Frontend copied here
│   ├── linux-x64/
│   ├── osx-x64/
│   └── osx-arm64/
│
└── src-tauri/
    └── target/release/bundle/         # Final installers
        ├── msi/
        │   └── Derot My Brain_0.1.0_x64_en-US.msi  ⭐
        └── nsis/
            └── Derot My Brain_0.1.0_x64-setup.exe  ⭐
```

---

## Quick Reference

| Command | Purpose |
|---------|---------|
| `.\build-tauri.ps1 --WindowsOnly` | Build Windows installer (recommended) |
| `.\build-tauri.ps1` | Build all platforms |
| `npm run tauri:dev` | Development mode |
| `rustc --version` | Check Rust installed |

**Ready to build? Run**: `.\scripts\build-tauri.ps1 --WindowsOnly` 🚀
