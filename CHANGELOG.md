# Changelog

## 0.1.2 — Unreleased

- Keeps `0.1.2` as the active development version because the previous Toggle Console attempt was not released.
- Reworked native **Toggle Console** handling after runtime testing showed that the first `0.1.2` startup-only handle synchronization was not sufficient.
- Refreshes the Arma `Process` object before reading `MainWindowHandle` so a cached early window handle is not trusted indefinitely.
- Adds explicit top-level window discovery with `EnumWindows` / `GetWindowThreadProcessId` for windows owned by the running Arma process.
- Keeps `AttachConsole` / `GetConsoleWindow` as an additional console-specific fallback instead of using it as the only repair path.
- Detects `PseudoConsoleWindow` and attempts to resolve its root owner for Windows Terminal / ConPTY-style hosting.
- Continuously monitors the resolved native window for the lifetime of the Arma process instead of stopping after a short startup window.
- Synchronizes a valid resolved HWND into the matching WindowsGSM `ServerMetadata.MainWindow` and persists it to the server's `windowsIntPtr` cache.
- Adds `arma3-toggle-console.log` in the WindowsGSM server cache folder so failed runtime detection can be diagnosed from the exact discovery path and Win32 error.
- Keeps `RedirectStandardOutput` disabled because WindowsGSM intentionally bypasses its native Toggle Console action when standard output is redirected.
- Keeps the read-only RPT-based Embedded Console implementation introduced in `0.1.1` unchanged.
- Keeps graceful-stop handling conservative: a normal process-window close is attempted first; a classic console window may receive `WM_CLOSE`; pseudo-terminal windows are not closed by the plugin.
- Updated README and installation documentation to distinguish the released `0.1.1` build from the unreleased `0.1.2` development build and to describe the new diagnostics.
- Source-reviewed the implementation against WindowsGSM's current Toggle Console, server-metadata and cache flow and against the relevant Win32/.NET window-handle APIs. Final Windows/Arma runtime confirmation is still required before `0.1.2` release sign-off.

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
