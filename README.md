<p align="center">
  <img src="ARMA3.cs/ARMA3.png" alt="Arma 3" width="160">
</p>

<h1 align="center">WindowsGSM.ARMA3</h1>

<p align="center">
  MeFriendos build for running an Arma 3 Dedicated Server with WindowsGSM.
</p>

<p align="center">
  <a href="https://github.com/WindowsGSM/WindowsGSM"><img src="https://img.shields.io/badge/WindowsGSM-%E2%89%A51.21-38CDD4" alt="WindowsGSM 1.21+"></a>
  <a href="CHANGELOG.md"><img src="https://img.shields.io/badge/version-0.1.0-9EFF99" alt="Version 0.1.0"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="MIT License"></a>
</p>

This plugin installs, updates and runs the Arma 3 Dedicated Server through SteamCMD. The MeFriendos build keeps the familiar WindowsGSM workflow while adding a safer firewall policy, 64-bit server startup and read-only embedded-console output.

## Features

- Installs and updates the official Arma 3 Dedicated Server through SteamCMD.
- Starts `arma3server_x64.exe` instead of the legacy 32-bit executable.
- Supports read-only embedded WindowsGSM console output from stdout and stderr.
- Tries a normal console-window close before using process termination when stopping the server.
- Removes WindowsGSM's automatic application-wide firewall exception before the server starts listening.
- Leaves targeted manual firewall rules unchanged.
- Uses a `100`-port WindowsGSM instance increment to avoid overlapping ArmA port groups.
- Supports custom startup parameters for mods, server mods, profiles and custom configs.

## Quick overview

| Setting | Value |
| --- | --- |
| SteamCMD App ID | `233780` |
| Start executable | `arma3server_x64.exe` |
| Default game port | `2302/UDP` |
| Default query port | `2303/UDP` |
| Default WindowsGSM port increment | `100` |
| Installation login | Steam account |
| Default parameters | `-profiles=ArmaHosts -config=server.cfg` |
| Firewall ports | Manual configuration only |
| Embedded console | Read-only stdout/stderr |

## Requirements

- WindowsGSM 1.21 or newer
- Supported 64-bit Windows installation
- Administrator rights for WindowsGSM when the firewall safety check is enabled
- A Steam account usable by SteamCMD for installation and updates

The Arma 3 Dedicated Server package uses Steam App ID `233780`. A dedicated Steam account without purchases is recommended for server administration.

## Plugin installation

1. Download the latest release archive.
2. Extract the complete `ARMA3.cs` folder into `<WindowsGSM>\plugins\`.
3. Click **Reload Plugins** or restart WindowsGSM.
4. Add **Arma 3 Dedicated Server** in WindowsGSM.
5. Enter the Steam credentials requested by WindowsGSM and click **Install**.
6. Configure `server.cfg`, your profile and any `-mod` / `-serverMod` parameters.
7. Create only the required manual firewall rules.
8. Start the server.

## Updating Arma 3

1. Stop the server.
2. Back up `server.cfg`, profiles, missions and any locally managed server files.
3. Click **Update** in WindowsGSM.
4. Start the server and review the RPT / console output.

SteamCMD updates the dedicated-server files. The plugin does not intentionally rewrite your mission, Altis Life configuration, mod list, server.cfg or profile files.

## Security: automatic port opening is disabled

WindowsGSM normally creates an application firewall exception for the configured start executable before calling a plugin's start method. An application-wide exception is broader than the small UDP port range ArmA actually requires.

This build removes the automatic exception for this server's exact `arma3server_x64.exe` path through the same Windows Firewall API family used by WindowsGSM. After removal, the plugin checks the application exception list again. If the exception cannot be verified as removed, startup is blocked.

The plugin does **not** create game-port rules and does not remove manually configured port-specific rules. Configure only the ports your server actually uses.

For the default ArmA 3 port layout, the relevant incoming ports are:

| Port | Protocol | Purpose |
| --- | --- | --- |
| `2302` | UDP | Game traffic / VON |
| `2303` | UDP | Steam query (`+1`) |
| `2304` | UDP | Steam master (`+2`) |
| `2305` | UDP | VON allocation (`+3`) |
| `2306` | UDP | BattlEye (`+4`) |

When the game port changes, the related ports move with the same offsets. For multiple ArmA servers, keep separate port groups; this plugin uses a `100`-port WindowsGSM increment so the next default instance becomes `2402`, then `2502`, and so on.

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

## Embedded console

ArmA 3's dedicated-server console is primarily an output console. This plugin therefore embeds **stdout and stderr only** into WindowsGSM and intentionally does not redirect standard input.

That means:

- server output can appear in the WindowsGSM console;
- the native server console remains available in the process model;
- typing arbitrary local commands into the WindowsGSM console is not treated as a supported ArmA administration interface;
- in-game `#` admin commands and BattlEye RCon remain separate mechanisms.

The stop action first asks the native console window to close normally and waits for the process to exit. Only if that does not work does it fall back to terminating the process, preserving the old plugin's last-resort behavior.

## Profiles and server name

For compatibility with the original plugin, the WindowsGSM **Server Name** is passed as ArmA's `-name` parameter. In ArmA, `-name` selects the **profile name**; it is not the public browser hostname.

The public server name belongs in `server.cfg`, for example:

```cpp
hostname = "MeFriendos Altis Life";
```

The default `-profiles=ArmaHosts` parameter keeps profile/log data below the server installation. You can replace it with another relative or absolute path in WindowsGSM's additional parameters when required.

## Mods and Altis Life

Use WindowsGSM's additional startup parameters for your existing setup, for example:

```text
-mod=@CBA_A3;@YourClientMod -serverMod=@YourServerMod
```

The plugin does not scan, download, reorder or modify ArmA mods. This is intentional so established Altis Life installations and custom server-side extensions remain under administrator control.

For very long mod command lines, ArmA also supports startup parameter files through `-par=`.

## Troubleshooting

### The server does not start and reports a firewall error

Run WindowsGSM as administrator and check Windows Firewall for an application exception pointing to this server's exact `arma3server_x64.exe`. The plugin blocks startup when it cannot verify the automatic exception was removed.

### Players cannot connect

Verify the complete UDP port group derived from the configured game port, router/NAT forwarding and any provider firewall. With the default port, check `2302-2306/UDP`.

### The server appears with the wrong public name

Change `hostname` in `server.cfg`. WindowsGSM's Server Name is used as the ArmA profile name for compatibility and does not control the public hostname.

### Embedded console shows output but commands do not work

This is expected. Embedded console support is intentionally read-only for ArmA 3. Use supported in-game administration or RCon mechanisms for commands.

### Profile or RPT files are in an unexpected location

Check `-profiles=` and `-name=` in the effective startup parameters. ArmA writes logs to the configured server profile location.

## Testing checklist

- WindowsGSM loads `ARMA3.cs` without a plugin compilation error.
- Install and Update complete through SteamCMD App ID `233780`.
- The plugin starts `arma3server_x64.exe` with the configured game port and additional parameters.
- Embedded Console can be enabled in WindowsGSM and receives available ArmA stdout/stderr output.
- The server still starts normally when Embedded Console is disabled.
- Stop requests a normal window close before falling back to process termination.
- No WindowsGSM application-wide firewall exception remains for this server's `arma3server_x64.exe` after startup.
- Port-specific manual firewall rules remain unchanged.
- A firewall-cleanup failure prevents the server process from starting.
- Default multi-instance allocation uses `2302`, `2402`, `2502`, etc.
- The configured ArmA profile and existing Altis Life/mod parameters still load correctly.

## Project links

- Source: [PapaGordon/WindowsGSM.ARMA3-MeFriendos](https://github.com/PapaGordon/WindowsGSM.ARMA3-MeFriendos)
- Original plugin: [BattlefieldDuck/WindowsGSM.ARMA3](https://github.com/BattlefieldDuck/WindowsGSM.ARMA3)
- 64-bit fork reference: [MildlyInterested/WindowsGSM.ARMA3](https://github.com/MildlyInterested/WindowsGSM.ARMA3)
- WindowsGSM: [WindowsGSM/WindowsGSM](https://github.com/WindowsGSM/WindowsGSM)
- Arma 3 Dedicated Server documentation: [Bohemia Interactive Community Wiki](https://community.bohemia.net/wiki/Arma_3:_Dedicated_Server)
- Community: [mefriendos.de](https://mefriendos.de)

This is an independent community plugin. It is not affiliated with or endorsed by Bohemia Interactive or WindowsGSM.

## License

Based on the original MIT-licensed WindowsGSM.ARMA3 plugin. The original copyright and MIT license notice are retained in [LICENSE](LICENSE).
