using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WindowsGSM.Functions;
using WindowsGSM.GameServer.Engine;
using WindowsGSM.GameServer.Query;

namespace WindowsGSM.Plugins
{
    // MeFriendos build based on WindowsGSM.ARMA3 by BattlefieldDuck.
    public class ARMA3 : SteamCMDAgent
    {
        private const uint WM_CLOSE = 0x0010;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        // - Plugin Details
        public Plugin Plugin = new Plugin
        {
            name = "WindowsGSM.ARMA3",
            author = "MeFriendos",
            description = "WindowsGSM plugin for Arma 3 Dedicated Server (MeFriendos build)",
            version = "0.1.0",
            url = "https://github.com/PapaGordon/WindowsGSM.ARMA3-MeFriendos",
            color = "#9eff99"
        };

        // - Standard Constructor and properties
        public ARMA3(ServerConfig serverData) : base(serverData) => base.serverData = _serverData = serverData;
        private readonly ServerConfig _serverData;

        // - Settings properties for SteamCMD installer
        public override bool loginAnonymous => false;
        public override string AppId => "233780";

        // - Game server Fixed variables
        public override string StartPath => "arma3server_x64.exe";
        public string FullName = "Arma 3 Dedicated Server";
        public bool AllowsEmbedConsole = true;
        public int PortIncrements = 100;
        public object QueryMethod = new A2S();

        // - Game server default values
        public string Port = "2302";
        public string QueryPort = "2303";
        public string Defaultmap = "empty";
        public string Maxplayers = "64";
        public string Additional = "-profiles=ArmaHosts -config=server.cfg";

        // - Create a default cfg for the game server after installation
        public async void CreateServerCFG() { }

        private bool RemoveAutomaticFirewallException()
        {
            string programPath = ServerPath.GetServersServerFiles(_serverData.ServerID, StartPath);

            try
            {
                Type managerType = Type.GetTypeFromProgID("HNetCfg.FwMgr");
                if (managerType == null)
                    return false;

                object manager = Activator.CreateInstance(managerType);
                object localPolicy = manager.GetType().InvokeMember(
                    "LocalPolicy", BindingFlags.GetProperty, null, manager, null);
                object currentProfile = localPolicy.GetType().InvokeMember(
                    "CurrentProfile", BindingFlags.GetProperty, null, localPolicy, null);
                object applications = currentProfile.GetType().InvokeMember(
                    "AuthorizedApplications", BindingFlags.GetProperty, null, currentProfile, null);

                IEnumerable entries = applications as IEnumerable;
                if (entries == null)
                    return false;

                bool found = false;
                foreach (object application in entries)
                {
                    string applicationPath = Convert.ToString(application.GetType().InvokeMember(
                        "ProcessImageFileName", BindingFlags.GetProperty, null, application, null));

                    if (string.Equals(applicationPath, programPath, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    return true;

                applications.GetType().InvokeMember(
                    "Remove", BindingFlags.InvokeMethod, null, applications, new object[] { programPath });

                applications = currentProfile.GetType().InvokeMember(
                    "AuthorizedApplications", BindingFlags.GetProperty, null, currentProfile, null);
                entries = applications as IEnumerable;
                if (entries == null)
                    return false;

                foreach (object application in entries)
                {
                    string applicationPath = Convert.ToString(application.GetType().InvokeMember(
                        "ProcessImageFileName", BindingFlags.GetProperty, null, application, null));

                    if (string.Equals(applicationPath, programPath, StringComparison.OrdinalIgnoreCase))
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        // - Start server function, return its Process to WindowsGSM
        public async Task<Process> Start()
        {
            string exePath = ServerPath.GetServersServerFiles(_serverData.ServerID, StartPath);
            if (!File.Exists(exePath))
            {
                Error = StartPath + " not found (" + exePath + ")";
                return null;
            }

            if (!RemoveAutomaticFirewallException())
            {
                Error = "Automatic firewall access could not be disabled. Start WindowsGSM as administrator or remove the broad arma3server_x64.exe rule manually.";
                return null;
            }

            var param = new StringBuilder();
            param.Append(string.IsNullOrWhiteSpace(_serverData.ServerPort) ? string.Empty : $" -port={_serverData.ServerPort}");
            param.Append(string.IsNullOrWhiteSpace(_serverData.ServerName) ? string.Empty : $" -name=\"{_serverData.ServerName}\"");
            param.Append(string.IsNullOrWhiteSpace(_serverData.ServerParam) ? string.Empty : $" {_serverData.ServerParam}");

            var p = new Process
            {
                StartInfo =
                {
                    WindowStyle = ProcessWindowStyle.Minimized,
                    UseShellExecute = false,
                    WorkingDirectory = ServerPath.GetServersServerFiles(_serverData.ServerID),
                    FileName = exePath,
                    Arguments = param.ToString()
                },
                EnableRaisingEvents = true
            };

            if (_serverData.EmbedConsole)
            {
                // Arma's dedicated-server console is effectively output-only.
                // Keep the native console available for graceful window close and
                // redirect only output/error into the WindowsGSM console.
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                var serverConsole = new ServerConsole(_serverData.ServerID);
                p.OutputDataReceived += serverConsole.AddOutput;
                p.ErrorDataReceived += serverConsole.AddOutput;
            }

            try
            {
                p.Start();

                if (_serverData.EmbedConsole)
                {
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                }

                return p;
            }
            catch (Exception e)
            {
                Error = e.Message;
                return null;
            }
        }

        private bool RequestConsoleClose(Process p)
        {
            try
            {
                p.Refresh();
                if (p.MainWindowHandle != IntPtr.Zero && p.CloseMainWindow())
                    return true;

                if (!AttachConsole((uint)p.Id))
                    return false;

                IntPtr consoleWindow;
                try
                {
                    consoleWindow = GetConsoleWindow();
                }
                finally
                {
                    FreeConsole();
                }

                return consoleWindow != IntPtr.Zero &&
                       PostMessage(consoleWindow, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
            catch
            {
                return false;
            }
        }

        // - Stop server function
        public async Task Stop(Process p)
        {
            await Task.Run(() =>
            {
                if (p == null)
                    return;

                try
                {
                    if (p.HasExited)
                        return;

                    if (RequestConsoleClose(p) && p.WaitForExit(20000))
                        return;
                }
                catch
                {
                    // Fall back to process termination below.
                }

                try
                {
                    if (!p.HasExited)
                        p.Kill();
                }
                catch
                {
                    // Process may have exited between the checks.
                }
            });
        }
    }
}
