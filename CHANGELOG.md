# Changelog

## 0.1.2 — 2026-09-10

- Fixed cases where WindowsGSM's native **Toggle Console** button does nothing while `arma3server_x64.exe` is running normally.
- Added native console-window discovery after Arma startup instead of relying only on an early `Process.MainWindowHandle` value.
- Synchronizes the resolved native console HWND with the matching WindowsGSM `ServerMetadata.MainWindow` after WindowsGSM has registered the same Arma process.
- Keeps synchronizing through the final `Started` state to reduce races with WindowsGSM's own startup-time window-handle assignment.
- Persists the resolved console handle to the server's `windowsIntPtr` cache for the running instance.
- Keeps `RedirectStandardOutput` disabled so WindowsGSM does not intentionally bypass its native Toggle Console action.
- Reuses the native console-window resolver as a fallback for graceful shutdown when `Process.MainWindowHandle` is unavailable.
- Keeps the read-only RPT-based Embedded Console implementation from `0.1.1` unchanged.
- Clarified throughout the documentation that **Embed Console** and **Toggle Console** are separate features.
- Updated plugin version, GitHub frontpage, installation documentation and validation notes to `0.1.2`.
- Source-reviewed the new handling against WindowsGSM's current Toggle Console, server-metadata and cache flow. Final Windows/Arma runtime confirmation is still required before release sign-off.

## 0.1.1 — 2026-09-10

- Fixed the embedded console on Windows. Arma 3 Dedicated Server does not reliably expose its normal server log through redirected `stdout` / `stderr`.
- Replaced the non-working standard-stream redirect with a read-only live mirror of the active Arma `.rpt` file.
- Uses WindowsGSM's runtime `AllowsEmbedConsole` state instead of reading a stale configuration value inside `Start()`.
- Resolves relative, absolute and quoted `-profiles=` paths and falls back to `%LOCALAPPDATA%\Arma 3` when no profiles path is configured.
- Searches the configured profile root and direct profile subfolders for Arma RPT files.
- Takes an RPT snapshot before startup so quick restarts do not deliberately attach to an unchanged log from the previous process.
- Detects `-noLogs` and reports that the embedded console has no RPT source without blocking normal server startup.
- Keeps the native Arma process unredirected so WindowsGSM can retain normal window handling and the graceful-close fallback.
- Rechecked the existing `arma3server_x64.exe`, SteamCMD, firewall-cleanup and `100`-port increment behavior.
- Updated README and installation documentation to describe the actual RPT-based console implementation.
- Runtime-tested the RPT-based embedded console with WindowsGSM `v1.25.1.21` before release.

## 0.1.0 — 2026-09-10

- Created the MeFriendos build based on WindowsGSM.ARMA3 by BattlefieldDuck.
- Uses the 64-bit `arma3server_x64.exe` dedicated-server executable.
- Added an initial read-only embedded-console implementation using ArmA 3 stdout and stderr. This approach was replaced in `0.1.1` because it does not reliably provide Arma server output on Windows.
- Keeps the native server console available so WindowsGSM can request a normal window close before falling back to process termination.
- Replaced immediate-only process termination with graceful-close-first shutdown behavior.
- Removes WindowsGSM's automatic firewall application exception for the exact Arma 3 server executable before launch.
- Verifies that the automatic exception is gone before allowing the server process to start.
- Keeps manually configured port rules unchanged and does not open game ports automatically.
- Changed the WindowsGSM port allocation increment to `100` for safer multi-instance layouts.
- Added installation, security, port, troubleshooting and testing documentation.
