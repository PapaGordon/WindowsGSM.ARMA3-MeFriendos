VERSION 0.1.2 (UNRELEASED DEVELOPMENT BUILD)

WindowsGSM.ARMA3 - MeFriendos build
====================================

Purpose
-------
WindowsGSM plugin for Arma 3 Dedicated Server using arma3server_x64.exe.

Release status
--------------
The latest published GitHub release is 0.1.1.
The main branch currently contains the unreleased 0.1.2 development build for Toggle Console testing.

Installation
------------
Stable release:
1. Download the latest published GitHub release.
2. Copy the complete ARMA3.cs folder into the WindowsGSM plugins folder.
3. Reload plugins or restart WindowsGSM.
4. Add "Arma 3 Dedicated Server" in WindowsGSM.
5. Install/update through SteamCMD with the Steam account requested by WindowsGSM.
6. Configure server.cfg, profiles, missions and mod startup parameters.
7. Configure only the required Arma UDP ports manually in Windows Firewall/router/provider firewall.
8. Start the server.

Testing unreleased 0.1.2:
1. Download the source ZIP from the repository main branch.
2. Replace the installed ARMA3.cs plugin folder with the main-branch version.
3. Reload plugins or restart WindowsGSM.
4. Stop and start the Arma server so the new window monitor is attached to the new process.
5. Wait until WindowsGSM reports the server as started.
6. Test Toggle Console twice: show, then hide.
7. If it still fails, inspect the server cache file arma3-toggle-console.log.

Default parameters
------------------
-profiles=ArmaHosts -config=server.cfg

WindowsGSM Server Name is passed to Arma as -name for compatibility with the original plugin.
In Arma, -name selects the profile name. The public server browser name must be set with hostname= in server.cfg.

Console behavior
----------------
WindowsGSM has two separate console features for this plugin:

1. Embed Console
   Read-only Arma output mirrored from the active .rpt file into WindowsGSM.

2. Toggle Console
   Shows or hides the native Arma/console-host window selected through WindowsGSM's stored window handle.

Embedded Console
----------------
Arma 3 Dedicated Server on Windows does not reliably provide its normal server log through redirected stdout/stderr. Since version 0.1.1, the plugin leaves the Arma process unredirected and mirrors the active .rpt log into the WindowsGSM console.

The plugin resolves -profiles= values with relative, absolute and quoted paths. If -profiles= is not configured, it falls back to %LOCALAPPDATA%\Arma 3. It checks the profile root and direct profile subfolders for the active Arma RPT file.

On a quick restart, the plugin snapshots existing RPT files before launch so it does not deliberately attach to an unchanged log from the previous process.

If -noLogs is enabled, the server can still start but there is no RPT source for the embedded console. WindowsGSM displays a clear message in that case.

Standard input is intentionally not redirected. Use in-game # admin commands or RCon for administration.

Toggle Console development fix in 0.1.2
----------------------------------------
The first unreleased 0.1.2 attempt used a short startup-only AttachConsole/GetConsoleWindow synchronization. Runtime testing showed that this was not sufficient, so the implementation has been replaced while keeping the version number at 0.1.2.

The current development build now:
- calls Process.Refresh() before trusting Process.MainWindowHandle;
- explicitly searches top-level windows belonging to the Arma PID;
- uses AttachConsole/GetConsoleWindow only as an additional console fallback;
- recognizes PseudoConsoleWindow hosting and attempts to use its root owner;
- writes a valid result to WindowsGSM ServerMetadata.MainWindow and windowsIntPtr;
- continues monitoring while the Arma process is alive instead of stopping after 15 seconds;
- creates arma3-toggle-console.log in the server cache directory for diagnostics.

RedirectStandardOutput remains disabled. WindowsGSM intentionally ignores Toggle Console for processes whose standard output is redirected.

Diagnostic file
---------------
If Toggle Console still does nothing, check:

<WindowsGSM>\servers\<server-id>\cache\arma3-toggle-console.log

The file records the Arma PID, the discovery method, old/new HWND values and an AttachConsole Win32 error when applicable.

Stop behavior
-------------
The plugin refreshes the process and first tries the normal process-window close path.
If that is unavailable, a classic console window may receive WM_CLOSE.
PseudoConsoleWindow/terminal-host windows are deliberately not closed by this fallback.
The plugin waits up to 20 seconds after a successful close request before falling back to terminating the process.

Firewall behavior
-----------------
Automatic application-wide firewall access is intentionally disabled.

WindowsGSM creates a firewall application exception for arma3server_x64.exe before plugin Start() runs. This build removes that exact application exception through the Windows Firewall API and verifies that it is gone before starting the server.

The plugin does not create or remove your manual port-specific firewall rules.
If the safety check cannot verify the automatic exception was removed, startup is blocked. Run WindowsGSM as administrator.

Default incoming ports
----------------------
2302/UDP - game traffic / VON
2303/UDP - Steam query
2304/UDP - Steam master
2305/UDP - VON allocation
2306/UDP - BattlEye

The ports move together when the base game port changes.
For multiple Arma instances, this plugin uses a WindowsGSM PortIncrements value of 100: 2302, 2402, 2502, etc.

Recommended server.cfg security review
--------------------------------------
BattlEye = 1;
verifySignatures = 2;
allowedFilePatching = 0;
upnp = 0;

Do not blindly change allowedFilePatching on a server that uses Headless Clients.
Use strong private administration passwords and mission/mod-specific BattlEye filters where appropriate.

Validation
----------
The v0.1.1 RPT-based embedded console was runtime-tested with WindowsGSM v1.25.1.21 and a running Arma 3 Dedicated Server before release.

The current unreleased v0.1.2 Toggle Console implementation has been source-reviewed against WindowsGSM's current Toggle Console, server metadata and cache flow and the relevant .NET/Win32 window APIs. Final real-world validation still requires starting an Arma server on Windows and confirming that Toggle Console shows and hides the intended native window.

Important
---------
- The plugin does not rewrite server.cfg or Arma3Profile files.
- The plugin does not manage your Altis Life mission or mod list.
- The plugin does not open game ports automatically.
- Existing custom startup parameters are passed through unchanged.
- Embedded Console is read-only.
- Toggle Console is separate from Embedded Console.
- -noLogs disables the RPT source used by Embedded Console, not the native Toggle Console path.
- 0.1.2 is not released yet; main is the development/test source.
- Back up server configuration, profiles and missions before major updates.

Credits
-------
Based on WindowsGSM.ARMA3 by BattlefieldDuck.
64-bit implementation compared with the MildlyInterested fork.
MeFriendos build by PapaGordon / MeFriendos.

https://github.com/PapaGordon/WindowsGSM.ARMA3-MeFriendos
https://mefriendos.de
