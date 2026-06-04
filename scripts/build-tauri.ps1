#!/usr/bin/env pwsh
# PowerShell script for building complete Tauri desktop application
# Usage: .\build-tauri.ps1

param(
    [switch]$SkipFrontend,
    [switch]$SkipBackend,
    [switch]$SkipTauri,
    [switch]$WindowsOnly,
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Derot My Brain - Tauri Desktop Builder" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Set paths
$rootDir = Split-Path $PSScriptRoot -Parent
$frontendDir = Join-Path $rootDir "src\frontend"
$backendDir = Join-Path $rootDir "src\backend\DerotMyBrain.API"
$tauriDir = Join-Path $rootDir "src-tauri"
$publishedBackendDir = Join-Path $rootDir "published-backend"

# Step 1: Build Frontend
if (-not $SkipFrontend) {
    Write-Host "[1/4] Building React Frontend..." -ForegroundColor Green
    Push-Location $frontendDir
    try {
        Write-Host "  - Installing dependencies..." -ForegroundColor Gray
        npm install
        
        Write-Host "  - Building production bundle..." -ForegroundColor Gray
        npm run build
        
        Write-Host "  - Frontend build complete" -ForegroundColor Green
    }
    catch {
        Write-Host "  - Frontend build failed: $_" -ForegroundColor Red
        Pop-Location
        exit 1
    }
    finally {
        Pop-Location
    }
}
else {
    Write-Host '[1/4] Skipping frontend build (-SkipFrontend)' -ForegroundColor Yellow
}

# Step 2: Build Backend Executables
if (-not $SkipBackend) {
    Write-Host ""
    Write-Host "[2/4] Publishing .NET backend..." -ForegroundColor Green
    
    # Clean published-backend directory
    if (Test-Path $publishedBackendDir) {
        Remove-Item $publishedBackendDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $publishedBackendDir -Force | Out-Null
    
    Push-Location $backendDir
    
    # Define target runtimes
    $runtimes = @()
    if ($WindowsOnly) {
        $runtimes = @("win-x64")
        Write-Host "  - Building for Windows only" -ForegroundColor Gray
    }
    else {
        $runtimes = @("win-x64", "linux-x64", "osx-x64", "osx-arm64")
        Write-Host "  - Building for all platforms" -ForegroundColor Gray
    }
    
    foreach ($runtime in $runtimes) {
        Write-Host ""
        Write-Host "  Publishing for $runtime..." -ForegroundColor Cyan
        
        $outputPath = Join-Path $publishedBackendDir $runtime
        
        try {
            dotnet publish `
                --configuration $Configuration `
                --runtime $runtime `
                --self-contained true `
                --output $outputPath `
                /p:PublishSingleFile=true `
                /p:IncludeNativeLibrariesForSelfExtract=true `
                /p:EnableCompressionInSingleFile=true `
                /p:DebugType=None `
                /p:DebugSymbols=false
            
            if ($LASTEXITCODE -ne 0) {
                throw "dotnet publish failed with exit code $LASTEXITCODE"
            }
            
            Write-Host "  - $runtime published successfully" -ForegroundColor Green
        }
        catch {
            Write-Host "  - Failed to publish $runtime : $_" -ForegroundColor Red
            Pop-Location
            exit 1
        }
    }
    
    Pop-Location
}
else {
    Write-Host '[2/4] Skipping backend build (-SkipBackend)' -ForegroundColor Yellow
}

# Step 2.5: Copy frontend build to each published backend's wwwroot
if (-not $SkipFrontend) {
    Write-Host ""
    Write-Host "[2.5/4] Copying frontend to published backends..." -ForegroundColor Green
    
    $distDir = Join-Path $rootDir "dist"
    
    if (-not (Test-Path $distDir)) {
        Write-Host "  - Error: Frontend dist directory not found at $distDir" -ForegroundColor Red
        exit 1
    }
    
    # Get the list of runtimes (same as backend publish)
    $runtimesToCopy = @()
    if ($WindowsOnly) {
        $runtimesToCopy = @("win-x64")
    }
    else {
        $runtimesToCopy = @("win-x64", "linux-x64", "osx-x64", "osx-arm64")
    }
    
    foreach ($runtime in $runtimesToCopy) {
        $targetWwwroot = Join-Path $publishedBackendDir "$runtime\wwwroot"
        
        # Only copy if the backend directory exists
        if (Test-Path (Join-Path $publishedBackendDir $runtime)) {
            Write-Host "  - Copying dist to $runtime/wwwroot..." -ForegroundColor Cyan
            
            # Create wwwroot if it doesn't exist
            if (-not (Test-Path $targetWwwroot)) {
                New-Item -ItemType Directory -Path $targetWwwroot -Force | Out-Null
            }
            
            # Copy all frontend files
            Copy-Item -Path "$distDir\*" -Destination $targetWwwroot -Recurse -Force
            
            Write-Host "  - Frontend copied to $runtime" -ForegroundColor Green
        }
        else {
            Write-Host "  - Skipping $runtime (backend not published)" -ForegroundColor Yellow
        }
    }
}
else {
    Write-Host '[2.5/4] Skipping frontend copy (-SkipFrontend)' -ForegroundColor Yellow
}

# Step 3: Verify tauri.conf.json is intact (no longer auto-generated)
Write-Host ""
Write-Host "[3/4] Verifying Tauri configuration..." -ForegroundColor Green

$tauriConfigPath = Join-Path $tauriDir "tauri.conf.json"
if (-not (Test-Path $tauriConfigPath)) {
    Write-Host "  - ERROR: tauri.conf.json not found at $tauriConfigPath" -ForegroundColor Red
    exit 1
}
Write-Host "  - tauri.conf.json OK" -ForegroundColor Green


# Step 4: Build Tauri Application
if (-not $SkipTauri) {
    Write-Host ""
    Write-Host "[4/5] Building Tauri desktop application..." -ForegroundColor Green

    Push-Location $tauriDir
    try {
        # Check if Rust is installed
        $rustVersion = & rustc --version 2>$null
        if ($LASTEXITCODE -ne 0) {
            Write-Host ""
            Write-Host "  - Rust is not installed!" -ForegroundColor Red
            Write-Host "  Please install Rust from: https://www.rust-lang.org/tools/install" -ForegroundColor Yellow
            Write-Host "  After installing Rust, run this script again." -ForegroundColor Yellow
            exit 1
        }
        
        Write-Host "  - Rust version: $rustVersion" -ForegroundColor Gray
        
        # Install npm dependencies
        Write-Host "  - Installing Tauri dependencies..." -ForegroundColor Gray
        npm install
        
        # Build Tauri app
        Write-Host "  - Building Tauri application (this may take a while)..." -ForegroundColor Gray
        npm run tauri build
        
        if ($LASTEXITCODE -ne 0) {
            throw "Tauri build failed with exit code $LASTEXITCODE"
        }
        
        Write-Host "  - Tauri build complete" -ForegroundColor Green
    }
    catch {
        Write-Host "  - Tauri build failed: $_" -ForegroundColor Red
        Pop-Location
        exit 1
    }
    finally {
        Pop-Location
    }
} else {
    Write-Host ""
    Write-Host "[4/5] Skipping Tauri build (-SkipTauri)" -ForegroundColor Yellow
}

# Summary
Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Build Complete!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Tauri installers are located in:" -ForegroundColor White
Write-Host "  $tauriDir\target\release\bundle" -ForegroundColor Gray
Write-Host ""
Write-Host "Available installers:" -ForegroundColor White

$bundleDir = Join-Path $tauriDir "target\release\bundle"
if (Test-Path $bundleDir) {
    Get-ChildItem -Path $bundleDir -Recurse -Include "*.msi", "*.exe", "*.dmg", "*.app", "*.deb", "*.AppImage" | ForEach-Object {
        $size = $_.Length / 1MB
        Write-Host "  - $($_.Name) ($([math]::Round($size, 2)) MB)" -ForegroundColor Gray
    }
}
Write-Host ""
