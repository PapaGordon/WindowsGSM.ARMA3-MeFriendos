VERSION 0.1.2 (UNRELEASED - RUNTIME APPROVED)

WindowsGSM.ARMA3 - MeFriendos build
====================================

Purpose
-------
WindowsGSM plugin for Arma 3 Dedicated Server using arma3server_x64.exe.

Release status
--------------
The latest published plugin release is 0.1.1.
The main branch currently contains the unreleased 0.1.2 build.
Version 0.1.2 has passed runtime validation in the MeFriendos environment and is only unreleased because no GitHub 0.1.2 release/tag has been published yet.

Primary WindowsGSM target
-------------------------
MeFriendos currently uses:
Raziel7893/WindowsGSM v1.25.1.22
https://github.com/Raziel7893/WindowsGSM/releases/tag/v1.25.1.22

Raziel v1.25.1.22 targets .NET 8 on Windows and requires the .NET 8 Desktop Runtime according to that WindowsGSM release.

Installation
------------
Stable plugin release:
1. Download the latest published plugin release.
2. Copy the complete ARMA3.cs folder into the WindowsGSM plugins folder.
3. Reload plugins or restart WindowsGSM.
4. Add "Arma 3 Dedicated Server" in WindowsGSM.
5. Install/update through SteamCMD with the Steam account requested by WindowsGSM.
6. Configure server.cfg, profiles, missions and mod startup parameters.
7. Configure only the required Arma UDP ports manually in Windows Firewall/router/provider firewall.
8. Start the server.

Current 0.1.2 from main:
1. Download the source ZIP from the repository main branch.
2. Replace the installed ARMA3.cs plugin folder with the main-branch version.
3. Reload plugins or restart WindowsGSM.
4. Stop and start the Arma server so the native-window monitor attaches to the new process.
5. Wait until WindowsGSM reports the server as started.
6. Toggle Console can then be used to show and hide the native Arma console.

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
   Shows or hides the native Arma/classic console-host window selected through WindowsGSM's stored HWND.

Embedded Console
----------------
Arma 3 Dedicated Server on Windows does not reliably provide its normal server log through redirected stdout/stderr. Since version 0.1.1, the plugin leaves the Arma process unredirected and mirrors the active .rpt log into the WindowsGSM console.

The plugin resolves -profiles= values with relative, absolute and quoted paths. If -profiles= is not configured, it falls back to %LOCALAPPDATA%\Arma 3. It checks the profile root and direct profile subfolders for the active Arma RPT file.

On a quick restart, the plugin snapshots existing RPT files before launch so it does not deliberately attach to an unchanged log from the previous process.

If -noLogs is enabled, the server can still start but there is no RPT source for the embedded console. WindowsGSM displays a clear message in that case.

Standard input is intentionally not redirected. Use in-game # admin commands or RCon for administration.

Toggle Console fix in 0.1.2
---------------------------
The first unreleased 0.1.2 attempt used a short startup-only AttachConsole/GetConsoleWindow synchronization. Runtime testing showed that this was not sufficient, so the implementation was replaced while keeping the version number at 0.1.2.

The final 0.1.2 implementation is specifically designed and source-reviewed against Raziel7893/WindowsGSM v1.25.1.22.

Important difference in Raziel v1.25.1.22:
- Toggle Console persists a ShowConsole state.
- The old RedirectStandardOutput guard is commented out.
- The button toggles ShowConsole and applies ShowNormal/Hide to the cached WindowsGSM MainWindow HWND.

The plugin therefore:
- calls Process.Refresh() before trusting Process.MainWindowHandle;
- explicitly searches top-level windows belonging to the Arma PID;
- prefers ConsoleWindowClass and otherwise only accepts a visible process-owned fallback window;
- uses AttachConsole/GetConsoleWindow as an additional console fallback;
- rejects PseudoConsoleWindow as an unsafe native toggle target;
- writes a valid result to WindowsGSM ServerMetadata.MainWindow and windowsIntPtr;
- continues monitoring while the Arma process is alive instead of stopping after 15 seconds;
- detects Raziel's ShowConsole state through reflection;
- directly applies ShowNormal/Hide to the correctly resolved HWND when ShowConsole changes;
- creates arma3-toggle-console.log in the server cache directory for diagnostics.

Reflection is used for ShowConsole so the plugin does not require that Raziel-specific field at compile time. On WindowsGSM builds without ShowConsole, normal HWND synchronization remains available.

Runtime validation
------------------
Runtime validation PASSED on 2026-09-10 in the real MeFriendos environment:

WindowsGSM: Raziel7893/WindowsGSM v1.25.1.22
Arma process: arma3server_x64.exe
Toggle Console show: PASS
Toggle Console hide: PASS
Normal server operation: PASS
Runtime approval: PASS

The native Toggle Console repair is therefore approved for the primary MeFriendos WindowsGSM environment.

Diagnostic file
---------------
For troubleshooting on other systems, check:

<WindowsGSM>\servers\<server-id>\cache\arma3-toggle-console.log

Useful entries include:
- Toggle Console monitor started for PID ...; WindowsGSM ...
- Resolved HWND 0x... via ...
- WindowsGSM MainWindow updated from 0x... to 0x...
- Detected WindowsGSM ShowConsole state support
- Applied ShowConsole=True directly to HWND 0x...
- Applied ShowConsole=False directly to HWND 0x...

If no usable target is found, the log also records AttachConsole errors where available.

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

Validation summary
------------------
- v0.1.1 RPT-based Embedded Console: runtime-tested before release.
- v0.1.2 native Toggle Console: runtime-tested and approved with Raziel7893/WindowsGSM v1.25.1.22 on 2026-09-10.
- Show and hide both work correctly.
- The plugin remains version 0.1.2 and is still marked unreleased only until the GitHub release/tag is created.

Important
---------
- The plugin does not rewrite server.cfg or Arma3Profile files.
- The plugin does not manage your Altis Life mission or mod list.
- The plugin does not open game ports automatically.
- Existing custom startup parameters are passed through unchanged.
- Embedded Console is read-only.
- Toggle Console is separate from Embedded Console.
- -noLogs disables the RPT source used by Embedded Console, not native Toggle Console handling.
- Back up server configuration, profiles and missions before major updates.

Credits
-------
Based on WindowsGSM.ARMA3 by BattlefieldDuck.
64-bit implementation compared with the MildlyInterested fork.
Primary MeFriendos WindowsGSM environment: Raziel7893/WindowsGSM v1.25.1.22.
MeFriendos build by PapaGordon / MeFriendos.

https://github.com/PapaGordon/WindowsGSM.ARMA3-MeFriendos
https://github.com/Raziel7893/WindowsGSM/releases/tag/v1.25.1.22
https://mefriendos.de
