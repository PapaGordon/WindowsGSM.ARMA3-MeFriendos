<p align="center">
  <img src="ARMA3.cs/ARMA3.png" alt="Arma 3" width="160">
</p>

<h1 align="center">WindowsGSM.ARMA3</h1>

<p align="center">
  MeFriendos build for running an Arma 3 Dedicated Server with WindowsGSM.
</p>

<p align="center">
  <a href="https://github.com/Raziel7893/WindowsGSM/releases/tag/v1.25.1.22"><img src="https://img.shields.io/badge/WindowsGSM-Raziel%20v1.25.1.22-38CDD4" alt="Raziel WindowsGSM v1.25.1.22"></a>
  <a href="CHANGELOG.md"><img src="https://img.shields.io/badge/development-0.1.2-9EFF99" alt="Development 0.1.2"></a>
  <a href="https://github.com/PapaGordon/WindowsGSM.ARMA3-MeFriendos/releases/latest"><img src="https://img.shields.io/badge/latest%20release-0.1.1-blue" alt="Latest release 0.1.1"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="MIT License"></a>
</p>

This plugin installs, updates and runs the Arma 3 Dedicated Server through SteamCMD. The MeFriendos build keeps the familiar WindowsGSM workflow while adding 64-bit server startup, a safer firewall policy, a read-only embedded console backed by Arma's RPT log and improved native **Toggle Console** handling.

> **Development status:** `main` currently contains the unreleased `0.1.2` development build. The latest published plugin release is `0.1.1`. The primary MeFriendos target for `0.1.2` is **Raziel7893/WindowsGSM v1.25.1.22**. Final runtime confirmation of the new Toggle Console path is still required before release.

## Features

- Installs and updates the official Arma 3 Dedicated Server through SteamCMD.
- Starts `arma3server_x64.exe` instead of the legacy 32-bit executable.
- Mirrors the active Arma 3 `.rpt` log into the WindowsGSM embedded console.
- Keeps the native Arma process unredirected and treats Embedded Console and native Toggle Console as separate features.
- Refreshes and discovers the native Arma window through multiple Win32 fallback paths instead of trusting one early cached handle.
- Monitors the resolved native window for the lifetime of the running Arma process.
- Synchronizes the resolved HWND with WindowsGSM's `ServerMetadata.MainWindow` and `windowsIntPtr` cache.
- Integrates with Raziel WindowsGSM's persistent `ShowConsole` state so the plugin can directly apply the requested show/hide state to the resolved Arma window.
- Uses reflection for `ShowConsole`, retaining a handle-sync compatibility path on WindowsGSM builds that do not expose that field.
- Writes Toggle Console diagnostics to `arma3-toggle-console.log` in the server cache folder.
- Tries a normal console-window close before using process termination when stopping the server.
- Removes WindowsGSM's automatic application-wide firewall exception before the server starts listening.
- Leaves targeted manual firewall rules unchanged.
- Uses a `100`-port WindowsGSM instance increment to avoid overlapping Arma port groups.
- Supports custom startup parameters for mods, server mods, profiles and custom configs.

## Quick overview

| Setting | Value |
| --- | --- |
| Development version | `0.1.2` (unreleased) |
| Latest published plugin release | `0.1.1` |
| Primary WindowsGSM target | `Raziel7893/WindowsGSM v1.25.1.22` |
| SteamCMD App ID | `233780` |
| Start executable | `arma3server_x64.exe` |
| Default game port | `2302/UDP` |
| Default query port | `2303/UDP` |
| Default WindowsGSM port increment | `100` |
| Installation login | Steam account |
| Default parameters | `-profiles=ArmaHosts -config=server.cfg` |
| Firewall ports | Manual configuration only |
| Embedded console | Read-only RPT mirror |
| Toggle Console | Native Arma / classic console-host window |

## Requirements

- Primary MeFriendos environment: **Raziel7893/WindowsGSM v1.25.1.22**
- Windows x64
- Administrator rights for WindowsGSM when the firewall safety check is enabled
- A Steam account usable by SteamCMD for installation and updates
- For Raziel WindowsGSM v1.25.1.22: the **.NET 8 Desktop Runtime** required by that WindowsGSM build

The Arma 3 Dedicated Server package uses Steam App ID `233780`. A dedicated Steam account without purchases is recommended for server administration.

## Plugin installation

### Stable plugin release

1. Download the latest published plugin release archive.
2. Extract the complete `ARMA3.cs` folder into `<WindowsGSM>\plugins\`.
3. Click **Reload Plugins** or restart WindowsGSM.
4. Add **Arma 3 Dedicated Server** in WindowsGSM.
5. Enter the Steam credentials requested by WindowsGSM and click **Install**.
6. Configure `server.cfg`, your profile and any `-mod` / `-serverMod` parameters.
7. Create only the required manual firewall rules.
8. Start the server.

### Testing the unreleased 0.1.2 development build

Use **Code → Download ZIP** on the repository's `main` branch, copy the included `ARMA3.cs` folder into the WindowsGSM plugins folder, reload the plugin and fully restart the Arma server. Updating the plugin while the old Arma process is still running cannot retroactively attach the new window monitor to that existing process.

## Updating Arma 3

1. Stop the server.
2. Back up `server.cfg`, profiles, missions and any locally managed server files.
3. Click **Update** in WindowsGSM.
4. Start the server and review the RPT / embedded-console output.

SteamCMD updates the dedicated-server files. The plugin does not intentionally rewrite your mission, Altis Life configuration, mod list, `server.cfg` or profile files.

## Console behavior

WindowsGSM exposes two different console-related features for this plugin. They are intentionally separate:

- **Embed Console** shows read-only Arma server output inside WindowsGSM by following the active `.rpt` file.
- **Toggle Console** shows or hides the native Arma/classic console-host window selected by the HWND stored in WindowsGSM's server metadata.

### Embedded console

Arma 3 Dedicated Server on Windows does not provide dependable server output through redirected `stdout` / `stderr`. Since version `0.1.1`, this plugin therefore leaves the Arma process unredirected and follows the active `.rpt` log instead.

When **Embed Console** is enabled, the plugin:

- resolves the effective `-profiles=` path from the startup parameters;
- supports relative, absolute and quoted profile paths;
- falls back to `%LOCALAPPDATA%\Arma 3` when no `-profiles=` parameter is configured;
- watches the profile root and direct profile subfolders for the active Arma RPT file;
- mirrors newly written RPT lines into the normal WindowsGSM console;
- avoids attaching to an unchanged RPT left behind by a previous run;
- stops following the file when the associated Arma process exits;
- reports a clear message if `-noLogs` disables the RPT source.

This is intentionally **read-only**. Typing commands into the WindowsGSM console is not a supported Arma administration interface. Use supported in-game `#` admin commands or BattlEye RCon for administration.

The `0.1.1` RPT-based console path was runtime-tested with WindowsGSM `v1.25.1.21` and an Arma 3 Dedicated Server before release.

### Native Toggle Console — 0.1.2 development rework

The first unreleased `0.1.2` attempt used a short startup-only `AttachConsole()` synchronization. Runtime testing showed that this was not sufficient.

A second review against the **actual MeFriendos WindowsGSM build, Raziel7893 v1.25.1.22**, exposed an important difference from upstream WindowsGSM: Raziel's Toggle Console implementation persists a `ShowConsole` state and the old `RedirectStandardOutput` guard is commented out. The button toggles `ShowConsole`, stores the setting, hides the cached HWND briefly and then applies `ShowNormal` or `Hide` to that HWND.

The current `0.1.2` development build therefore uses this behavior instead of assuming the upstream implementation:

1. Calls `Process.Refresh()` before reading `Process.MainWindowHandle` so an early cached handle is not trusted indefinitely.
2. If that does not produce a usable HWND, enumerates top-level windows owned by the Arma PID.
3. During that fallback, prefers `ConsoleWindowClass`; otherwise only a visible process-owned window is accepted so hidden helper windows are not chosen accidentally.
4. If no process-owned target is available, probes the Arma console through `AttachConsole()` / `GetConsoleWindow()`.
5. Rejects `PseudoConsoleWindow` as a native toggle target because it may not represent a safe independently visible console window.
6. Writes a valid target to the matching WindowsGSM `ServerMetadata.MainWindow` and the server's `windowsIntPtr` cache.
7. Continues monitoring while the Arma process is alive so a stale or replaced HWND can be repaired later.
8. Detects Raziel's `ShowConsole` field through reflection. When the button changes that state, the plugin directly applies `ShowNormal` or `Hide` to the correctly resolved HWND.

The reflection-based `ShowConsole` lookup avoids a hard compile-time dependency on Raziel's extra metadata field. On builds without `ShowConsole`, the plugin falls back to normal handle synchronization only.

For troubleshooting, the monitor creates:

```text
<WindowsGSM>\servers\<server-id>\cache\arma3-toggle-console.log
```

The file records the Arma PID, reported WindowsGSM version, discovery path, HWND changes, whether `ShowConsole` support was detected, direct show/hide requests and `AttachConsole` errors. If Toggle Console still does nothing, this is the first file to inspect.

The current `0.1.2` implementation has been source-reviewed against **Raziel7893/WindowsGSM v1.25.1.22**. It remains **unreleased** until final Windows/Arma runtime testing succeeds.

## Profiles and server name

For compatibility with the original plugin, the WindowsGSM **Server Name** is passed as Arma's `-name` parameter. In Arma, `-name` selects the **profile name**; it is not the public browser hostname.

The public server name belongs in `server.cfg`, for example:

```cpp
hostname = "MeFriendos Altis Life";
```

The default `-profiles=ArmaHosts` parameter keeps profile/log data below the server installation. You can replace it with another relative or absolute path in WindowsGSM's additional parameters when required.

If you use `-noLogs`, Arma does not create the RPT source required for the embedded console mirror. The game server itself can still start, but WindowsGSM will not have RPT output to display. This does not by itself disable native Toggle Console handling.

## Security: automatic port opening is disabled

WindowsGSM normally creates an application firewall exception for the configured start executable before calling a plugin's `Start()` method. An application-wide exception is broader than the small UDP port range Arma actually requires.

This build removes the automatic exception for this server's exact `arma3server_x64.exe` path through the Windows Firewall API before launching the server. After removal, the plugin checks the application exception list again. If the exception cannot be verified as removed, startup is blocked.

The plugin does **not** create game-port rules and does not remove manually configured port-specific rules. Configure only the ports your server actually uses.

For the default Arma 3 port layout, the relevant incoming ports are:

| Port | Protocol | Purpose |
| --- | --- | --- |
| `2302` | UDP | Game traffic / VON |
| `2303` | UDP | Steam query (`+1`) |
| `2304` | UDP | Steam master (`+2`) |
| `2305` | UDP | VON allocation (`+3`) |
| `2306` | UDP | BattlEye (`+4`) |

When the game port changes, the related ports move with the same offsets. For multiple Arma servers, keep separate port groups; this plugin uses a `100`-port WindowsGSM increment so the next default instance becomes `2402`, then `2502`, and so on.

If your provider has a second network firewall or security group, configure the same narrow rules there as well. Do not expose administrative services simply because the game server is public.

## Recommended server.cfg hardening

The plugin deliberately does not overwrite `server.cfg`. For a normal public modded server, review at least these settings yourself:

```cpp
BattlEye = 1;
verifySignatures = 2;
allowedFilePatching = 0;
upnp = 0;
```

`allowedFilePatching = 1` can be required for certain Headless Client setups, so do not change it blindly on an existing server. Keep `passwordAdmin` and `serverCommandPassword` strong and private, and use BattlEye filters that are appropriate for your specific mission and mod stack.

## Stop behavior

The stop action first refreshes the process and tries the normal process-window close path. If no usable process window is exposed, the plugin may resolve a classic console window and post `WM_CLOSE` to it. A `PseudoConsoleWindow` is deliberately not closed by this fallback because closing a terminal host would be less safe than using the existing last-resort process termination behavior.

The plugin waits up to 20 seconds after a successful normal close request before falling back to terminating the Arma process.

## Mods and Altis Life

Use WindowsGSM's additional startup parameters for your existing setup, for example:

```text
-mod=@CBA_A3;@YourClientMod -serverMod=@YourServerMod
```

The plugin does not scan, download, reorder or modify Arma mods. This is intentional so established Altis Life installations and custom server-side extensions remain under administrator control.

For very long mod command lines, Arma also supports startup parameter files through `-par=`.

## Troubleshooting

### Toggle Console still does nothing on the unreleased 0.1.2 build

1. Confirm WindowsGSM is **Raziel7893 v1.25.1.22** and WindowsGSM loaded plugin version `0.1.2`.
2. Fully restart the Arma server after replacing/reloading the plugin.
3. Wait until WindowsGSM reports the server as started.
4. Try **Toggle Console** twice: show, then hide.
5. Open `<WindowsGSM>\servers\<server-id>\cache\arma3-toggle-console.log`.
6. Look for `Detected WindowsGSM ShowConsole state support` and a `Resolved HWND ...` entry.
7. When the button is clicked, the log should contain `Applied ShowConsole=True` / `Applied ShowConsole=False`.

If the embedded RPT console works but Toggle Console still does not, these are separate features. `-noLogs` affects the RPT-based embedded console, not the native window monitor.

### The server does not start and reports a firewall error

Run WindowsGSM as administrator and check Windows Firewall for an application exception pointing to this server's exact `arma3server_x64.exe`. The plugin blocks startup when it cannot verify the automatic exception was removed.

### Players cannot connect

Verify the complete UDP port group derived from the configured game port, router/NAT forwarding and any provider firewall. With the default port, check `2302-2306/UDP`.

### The server appears with the wrong public name

Change `hostname` in `server.cfg`. WindowsGSM's Server Name is used as the Arma profile name for compatibility and does not control the public hostname.

### Embedded console stays on "Waiting for Arma 3 RPT output"

Check the effective `-profiles=` startup parameter and verify that Arma is writing an `.rpt` file there or in a direct profile subfolder. Also make sure `-noLogs` is not enabled.

### Embedded console says `-noLogs` disables the RPT log

Remove `-noLogs` if you want RPT output mirrored into WindowsGSM. Leaving it enabled does not prevent the server process itself from starting; it only removes the log source used by this feature.

### Embedded console shows output but commands do not work

This is expected. Embedded console support is intentionally read-only for Arma 3. Use supported in-game administration or RCon mechanisms for commands.

### Profile or RPT files are in an unexpected location

Check `-profiles=` and `-name=` in the effective startup parameters. Arma writes logs to the configured server profile location.

## Testing checklist

The unreleased `0.1.2` source and documentation were rechecked against **Raziel7893/WindowsGSM v1.25.1.22**. Before release, validate:

- WindowsGSM loads `ARMA3.cs` without a plugin compilation error.
- Install and Update remain handled through SteamCMD App ID `233780`.
- The plugin starts `arma3server_x64.exe` with the configured game port and additional parameters.
- `Process.Refresh()` runs before a refreshed `MainWindowHandle` is trusted.
- Process-owned top-level windows can be discovered with `EnumWindows` / `GetWindowThreadProcessId` when needed.
- Hidden helper windows are not selected merely because they are the first window owned by the Arma PID.
- Console-host discovery falls back to `AttachConsole()` / `GetConsoleWindow()` when the Arma process itself does not expose a usable window.
- `PseudoConsoleWindow` is rejected as an unsafe native Toggle Console target.
- A resolved handle is written only to the WindowsGSM metadata entry tracking the same Arma PID.
- The resolved handle is persisted to the server `windowsIntPtr` cache.
- Window monitoring continues while the Arma process is alive so a stale/replaced handle can be repaired.
- Raziel's `ShowConsole` state is detected through reflection.
- Changing Toggle Console causes the monitor to apply the matching show/hide state to the resolved HWND.
- `arma3-toggle-console.log` is created and records the detection and visibility path.
- Embedded Console continues to use the RPT mirror introduced in `0.1.1`.
- Relative, absolute and quoted `-profiles=` values are resolved.
- `-noLogs` is detected and reported without blocking server startup.
- A quick restart does not deliberately attach to an unchanged RPT from the previous run.
- The RPT follower ends with the Arma process.
- Stop does not deliberately close a pseudo-terminal host and still falls back to process termination when graceful close is unavailable.
- No WindowsGSM application-wide firewall exception remains for this server's `arma3server_x64.exe` after successful startup.
- Port-specific manual firewall rules remain unchanged.
- A firewall-cleanup verification failure prevents the server process from starting.
- Default multi-instance allocation uses `2302`, `2402`, `2502`, etc.
- Existing profile, Altis Life and mod startup parameters continue to pass through unchanged.

The existing `0.1.1` RPT implementation was runtime-tested with WindowsGSM `v1.25.1.21`. The `0.1.2` Toggle Console rework remains an unreleased development build until the new native-window behavior is verified on your real `Raziel7893 v1.25.1.22` installation.

## Project links

- Source: [PapaGordon/WindowsGSM.ARMA3-MeFriendos](https://github.com/PapaGordon/WindowsGSM.ARMA3-MeFriendos)
- Primary WindowsGSM target: [Raziel7893/WindowsGSM v1.25.1.22](https://github.com/Raziel7893/WindowsGSM/releases/tag/v1.25.1.22)
- Original plugin: [BattlefieldDuck/WindowsGSM.ARMA3](https://github.com/BattlefieldDuck/WindowsGSM.ARMA3)
- 64-bit fork reference: [MildlyInterested/WindowsGSM.ARMA3](https://github.com/MildlyInterested/WindowsGSM.ARMA3)
- Original WindowsGSM project: [WindowsGSM/WindowsGSM](https://github.com/WindowsGSM/WindowsGSM)
- Arma 3 Dedicated Server documentation: [Bohemia Interactive Community Wiki](https://community.bohemia.net/wiki/Arma_3:_Dedicated_Server)
- Community: [mefriendos.de](https://mefriendos.de)

This is an independent community plugin. It is not affiliated with or endorsed by Bohemia Interactive or WindowsGSM.

## License

Based on the original MIT-licensed WindowsGSM.ARMA3 plugin. The original copyright and MIT license notice are retained in [LICENSE](LICENSE).
