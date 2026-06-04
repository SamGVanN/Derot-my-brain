# Derot My Brain – Tauri Desktop Integration Plan

## Objective

Package the existing architecture:

- ASP.NET Core (self-contained executable)
- React frontend served from `wwwroot`

Into a **true native desktop application** using **Tauri**, while:

- Preserving Clean Architecture
- Keeping backend/domain untouched
- Remaining cross-platform (Windows / macOS / Linux)
- Avoiding unnecessary technology

---

# 1. Architectural Decision

## ✅ Chosen Strategy: Tauri as Native Wrapper

Tauri will act purely as a **native shell** that:

1. Launches the ASP.NET Core executable locally
2. Opens a WebView pointing to `http://127.0.0.1:<fixed-port>`

Final runtime structure:

```
Tauri (Rust shell)
   └── WebView
         └── http://127.0.0.1:5173
                └── ASP.NET Core (self-contained)
                        └── React (from wwwroot)
```

## ❌ Kotlin Decision

Kotlin is NOT required.

Reasons:
- Tauri uses Rust internally.
- Frontend is already React.
- Backend is ASP.NET Core.
- Kotlin is only relevant for Android native or JVM ecosystems.

Conclusion: **Do not use Kotlin.**

---

# 2. Backend Preparation

## 2.1 Publish ASP.NET Core as Self-Contained

Example (Windows x64):

```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

Repeat for:
- win-x64
- linux-x64
- osx-x64
- osx-arm64

## 2.2 Force a Fixed Port

In `Program.cs`:

```csharp
builder.WebHost.UseUrls("http://127.0.0.1:5173");
```

Requirements:
- No dynamic ports
- No HTTPS required (localhost only)
- CORS can be disabled

## 2.3 SQLite Path

Ensure database path is relative to executable:

```csharp
var dbPath = Path.Combine(AppContext.BaseDirectory, "derot-my-brain.db");
```

This ensures portability inside the Tauri bundle.

---

# 3. Frontend Build

Build React in production mode:

```bash
npm run build
```

Output must be copied into:

```
DerotMyBrain.API/wwwroot
```

ASP.NET Core continues serving the frontend.

No change in frontend architecture is required.

---

# 4. Tauri Initialization

At project root:

```bash
npm create tauri-app
```

Choose:
- Frontend: Vanilla (we are not using Tauri frontend)
- Package manager: npm

Then:

```bash
cd src-tauri
```

---

# 5. Configure Tauri to Load Local Backend

In `tauri.conf.json`:

```json
{
  "build": {
    "devPath": "http://127.0.0.1:5173",
    "distDir": null
  }
}
```

We are NOT using `distDir` because the frontend is served by ASP.NET.

---

# 6. Launch ASP.NET Process from Tauri

In `src-tauri/src/main.rs`:

```rust
use std::process::Command;

fn main() {
    Command::new("DerotMyBrain.API.exe")
        .spawn()
        .expect("Failed to start backend");

    tauri::Builder::default()
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
```

Notes:
- Executable name must match platform
- On macOS/Linux, adjust filename
- Backend executable must be bundled as a resource

---

# 7. Bundle Configuration

In `tauri.conf.json`, include backend executable in bundle resources:

```json
{
  "bundle": {
    "resources": [
      "../published-backend/*"
    ]
  }
}
```

Ensure:
- Correct relative paths
- Per-platform backend binary is used

---

# 8. Build Application

```bash
tauri build
```

Outputs:

- Windows → .msi
- macOS → .app
- Linux → .deb / .AppImage

---

# 9. Clean Architecture Impact

No changes required in:

- Core
- Infrastructure
- API
- Frontend layering

Tauri acts strictly as an outer wrapper.

All Clean Architecture rules remain valid.

---

# 10. Security Considerations

Since the API runs on localhost only:

- Disable CORS
- No external exposure
- No need for production HTTPS

The application behaves like a desktop application.

---

# 11. Future Considerations

## 11.1 Auto-Update

Tauri supports auto-update mechanism.
This can be added later.

## 11.2 Mobile

Tauri is desktop-focused.
For mobile (Android/iOS), future options:

- React Native
- .NET MAUI
- Native WebView wrappers

Mobile is out of scope for this phase.

---

# Final Recommendation

Phase 1:
- Implement Tauri desktop wrapper
- Stabilize cross-platform packaging