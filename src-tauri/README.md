# Derot My Brain - Native Desktop Application

This directory contains the Tauri wrapper for Derot My Brain, which provides a native desktop shell around the ASP.NET Core backend and React frontend.

## Architecture

```
Tauri (Rust shell)
   └── WebView → http://127.0.0.1:5173
          └── ASP.NET Core (self-contained backend)
                  └── React (from wwwroot)
```

## Key Components

- **lib.rs**: Backend process management and Tauri initialization
- **tauri.conf.json**: Tauri application configuration
- **Cargo.toml**: Rust dependencies
- **icons/**: Application icons for all platforms

## How it Works

1. Tauri launches the ASP.NET Core backend executable
2. Sets `TAURI_BUNDLED=true` environment variable
3. Waits for backend to be ready (health check)
4. Opens a WebView pointing to http://127.0.0.1:5173
5. On exit, gracefully shuts down the backend process

## Building

See the root `scripts/build-tauri.ps1` for the complete build process.

## Development

To run in development mode (requires backend to be running separately):
```bash
npm run tauri:dev
```

Note: You must start the backend first:
```bash
cd src/backend/DerotMyBrain.API
dotnet run
```
