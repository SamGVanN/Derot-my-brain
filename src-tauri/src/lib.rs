use std::env;
use std::net::TcpStream;
use std::path::PathBuf;
use std::process::{Child, Command};
use std::sync::Mutex;
use std::thread;
use std::time::{Duration, Instant};

use tauri::Manager;
use tauri::RunEvent;

/// Holds the backend child process so we can kill it on app exit.
struct BackendGuard(Mutex<Option<Child>>);

impl BackendGuard {
    fn shutdown(&self) {
        if let Ok(mut lock) = self.0.lock() {
            if let Some(mut child) = lock.take() {
                eprintln!("[Tauri] Shutting down backend (PID {})...", child.id());
                let _ = child.kill();
                let _ = child.wait();
                eprintln!("[Tauri] Backend stopped.");
            }
        }
    }
}

/// Find the backend executable.
///
/// Installed layout (NSIS):
///   <install_dir>/derot-my-brain.exe
///   <install_dir>/_up_/published-backend/win-x64/DerotMyBrain.API.exe
///
/// Development layout:
///   <repo>/published-backend/win-x64/DerotMyBrain.API.exe
fn find_backend() -> Result<PathBuf, String> {
    let exe_name = if cfg!(target_os = "windows") {
        "DerotMyBrain.API.exe"
    } else {
        "DerotMyBrain.API"
    };

    let platform = if cfg!(target_os = "windows") {
        "win-x64"
    } else if cfg!(target_os = "linux") {
        "linux-x64"
    } else if cfg!(target_arch = "aarch64") {
        "osx-arm64"
    } else {
        "osx-x64"
    };

    // 1. Installed app: look in _up_/published-backend/<platform>/ next to the Tauri exe.
    //    This is where NSIS places bundled resources.
    if let Ok(exe) = env::current_exe() {
        if let Some(exe_dir) = exe.parent() {
            let installed_path = exe_dir
                .join("_up_")
                .join("published-backend")
                .join(platform)
                .join(exe_name);

            if installed_path.exists() {
                eprintln!("[Tauri] Backend found (installed _up_ dir): {:?}", installed_path);
                return Ok(installed_path);
            } else {
                eprintln!("[Tauri] Not found at installed path: {:?}", installed_path);
            }

            // 1b. Also check directly next to the exe (MSI layout or manual placement)
            let adjacent_path = exe_dir.join(exe_name);
            if adjacent_path.exists() {
                eprintln!("[Tauri] Backend found (adjacent to exe): {:?}", adjacent_path);
                return Ok(adjacent_path);
            }
        }
    }

    // 2. Development fallback: published-backend/<platform>/ relative to cwd (repo root)
    let dev_path = env::current_dir()
        .unwrap_or_default()
        .join("published-backend")
        .join(platform)
        .join(exe_name);

    if dev_path.exists() {
        eprintln!("[Tauri] Backend found (dev path): {:?}", dev_path);
        return Ok(dev_path);
    }

    Err(format!(
        "Cannot find '{}'. Searched:\n  (1) _up_/published-backend/{}/{}\n  (2) Dev: {:?}",
        exe_name, platform, exe_name, dev_path
    ))
}

/// Block until port 45123 accepts TCP connections, or the timeout expires.
fn wait_for_backend(timeout: Duration) -> bool {
    let addr: std::net::SocketAddr = "127.0.0.1:45123".parse().unwrap();
    let deadline = Instant::now() + timeout;
    let start = Instant::now();

    eprintln!(
        "[Tauri] Waiting for backend on port 45123 (timeout: {}s)...",
        timeout.as_secs()
    );

    while Instant::now() < deadline {
        match TcpStream::connect_timeout(&addr, Duration::from_millis(200)) {
            Ok(_) => {
                eprintln!(
                    "[Tauri] Backend ready after {:.1}s",
                    start.elapsed().as_secs_f32()
                );
                return true;
            }
            Err(_) => thread::sleep(Duration::from_millis(150)),
        }
    }

    eprintln!(
        "[Tauri] WARNING: Backend not ready after {}s.",
        timeout.as_secs()
    );
    false
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    let app = tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .setup(|app| {
            // Find and spawn the backend process during app setup.
            // The UI will not appear until setup() returns.
            match find_backend() {
                Ok(path) => {
                    // Set working directory to the backend's own folder so
                    // appsettings.json, data/, Logs/ resolve correctly.
                    let backend_dir = path
                        .parent()
                        .map(|p| p.to_path_buf())
                        .unwrap_or_default();

                    eprintln!("[Tauri] Launching backend from: {:?}", path);
                    eprintln!("[Tauri] Working dir: {:?}", backend_dir);

                    #[cfg(unix)]
                    {
                        use std::os::unix::fs::PermissionsExt;
                        if let Ok(metadata) = std::fs::metadata(&path) {
                            let mut perms = metadata.permissions();
                            // Add execute permission for owner, group, and others
                            perms.set_mode(perms.mode() | 0o111);
                            if let Err(e) = std::fs::set_permissions(&path, perms) {
                                eprintln!("[Tauri] WARNING: Failed to set execute permissions on backend: {}", e);
                            }
                        }
                    }

                    match Command::new(&path)
                        .current_dir(&backend_dir)
                        .spawn()
                    {
                        Ok(child) => {
                            eprintln!("[Tauri] Backend spawned (PID: {})", child.id());

                            // Wait for it to be ready (up to 30s for DB migrations).
                            wait_for_backend(Duration::from_secs(30));

                            app.manage(BackendGuard(Mutex::new(Some(child))));
                        }
                        Err(e) => {
                            eprintln!("[Tauri] ERROR: Failed to spawn backend: {}", e);
                            app.manage(BackendGuard(Mutex::new(None)));
                        }
                    }
                }
                Err(e) => {
                    eprintln!("[Tauri] ERROR: {}", e);
                    app.manage(BackendGuard(Mutex::new(None)));
                }
            }

            Ok(())
        })
        .build(tauri::generate_context!())
        .expect("error while building tauri application");

    // Run the app event loop. The RunEvent::Exit callback is
    // guaranteed to fire when the application is shutting down,
    // regardless of how the user closes the window.
    app.run(|app_handle, event| {
        if let RunEvent::Exit = event {
            if let Some(guard) = app_handle.try_state::<BackendGuard>() {
                guard.shutdown();
            }
        }
    });
}
