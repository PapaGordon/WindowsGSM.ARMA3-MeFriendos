VERSION 0.1.1

WindowsGSM.ARMA3 - MeFriendos build
====================================

Purpose
-------
WindowsGSM plugin for Arma 3 Dedicated Server using arma3server_x64.exe.

Installation
------------
1. Copy the complete ARMA3.cs folder into the WindowsGSM plugins folder.
2. Reload plugins or restart WindowsGSM.
3. Add "Arma 3 Dedicated Server" in WindowsGSM.
4. Install/update through SteamCMD with the Steam account requested by WindowsGSM.
5. Configure server.cfg, profiles, missions and mod startup parameters.
6. Configure only the required Arma UDP ports manually in Windows Firewall/router/provider firewall.
7. Start the server.

Default parameters
------------------
-profiles=ArmaHosts -config=server.cfg

WindowsGSM Server Name is passed to Arma as -name for compatibility with the original plugin.
In Arma, -name selects the profile name. The public server browser name must be set with hostname= in server.cfg.

Embedded console
----------------
Embedded Console is supported as read-only output.

Arma 3 Dedicated Server on Windows does not reliably provide its normal server log through redirected stdout/stderr. Version 0.1.1 therefore leaves the Arma process unredirected and mirrors the active .rpt log into the WindowsGSM console.

The plugin resolves -profiles= values with relative, absolute and quoted paths. If -profiles= is not configured, it falls back to %LOCALAPPDATA%\Arma 3. It checks the profile root and direct profile subfolders for the active Arma RPT file.

On a quick restart, the plugin snapshots existing RPT files before launch so it does not deliberately attach to an unchanged log from the previous process.

If -noLogs is enabled, the server can still start but there is no RPT source for the embedded console. WindowsGSM displays a clear message in that case.

Standard input is intentionally not redirected. Use in-game # admin commands or RCon for administration.

Stop behavior
-------------
WindowsGSM first asks the native Arma console window to close and waits up to 20 seconds.
If the server does not exit, the plugin falls back to terminating the process.

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

Important
---------
- The plugin does not rewrite server.cfg or Arma3Profile files.
- The plugin does not manage your Altis Life mission or mod list.
- The plugin does not open game ports automatically.
- Existing custom startup parameters are passed through unchanged.
- Embedded Console is read-only.
- -noLogs disables the RPT source used by Embedded Console.
- Back up server configuration, profiles and missions before major updates.

Credits
-------
Based on WindowsGSM.ARMA3 by BattlefieldDuck.
64-bit implementation compared with the MildlyInterested fork.
MeFriendos build by PapaGordon / MeFriendos.

https://github.com/PapaGordon/WindowsGSM.ARMA3-MeFriendos
https://mefriendos.de
