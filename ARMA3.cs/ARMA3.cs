using System;
using System.Collections;
using System.Collections.Generic;
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

        private static readonly ConstructorInfo DataReceivedEventArgsConstructor =
            typeof(DataReceivedEventArgs).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(string) },
                null);

        // - Plugin Details
        public Plugin Plugin = new Plugin
        {
            name = "WindowsGSM.ARMA3",
            author = "MeFriendos",
            description = "WindowsGSM plugin for Arma 3 Dedicated Server (MeFriendos build)",
            version = "0.1.1",
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

        private static string GetStartParameterValue(string arguments, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(arguments) || string.IsNullOrWhiteSpace(parameterName))
                return null;

            string marker = "-" + parameterName + "=";
            int index = arguments.LastIndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return null;

            int valueStart = index + marker.Length;
            if (valueStart >= arguments.Length)
                return string.Empty;

            bool valueQuoted = arguments[valueStart] == '"';
            bool wholeArgumentQuoted = index > 0 && arguments[index - 1] == '"';

            if (valueQuoted)
            {
                valueStart++;
                int valueEnd = arguments.IndexOf('"', valueStart);
                if (valueEnd < 0)
                    valueEnd = arguments.Length;

                return arguments.Substring(valueStart, valueEnd - valueStart);
            }

            if (wholeArgumentQuoted)
            {
                int valueEnd = arguments.IndexOf('"', valueStart);
                if (valueEnd < 0)
                    valueEnd = arguments.Length;

                return arguments.Substring(valueStart, valueEnd - valueStart);
            }

            int end = valueStart;
            while (end < arguments.Length && !char.IsWhiteSpace(arguments[end]))
                end++;

            return arguments.Substring(valueStart, end - valueStart).Trim('"');
        }

        private static bool HasStartSwitch(string arguments, string switchName)
        {
            if (string.IsNullOrWhiteSpace(arguments) || string.IsNullOrWhiteSpace(switchName))
                return false;

            string marker = "-" + switchName;
            int searchFrom = 0;

            while (searchFrom < arguments.Length)
            {
                int index = arguments.IndexOf(marker, searchFrom, StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                    return false;

                int end = index + marker.Length;
                bool leftBoundary = index == 0 || char.IsWhiteSpace(arguments[index - 1]) || arguments[index - 1] == '"';
                bool rightBoundary = end >= arguments.Length || char.IsWhiteSpace(arguments[end]) || arguments[end] == '"';

                if (leftBoundary && rightBoundary)
                    return true;

                searchFrom = index + marker.Length;
            }

            return false;
        }

        private string GetRptDirectory(string arguments)
        {
            string configuredPath = GetStartParameterValue(arguments, "profiles");

            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Arma 3");
            }

            configuredPath = Environment.ExpandEnvironmentVariables(configuredPath.Trim());

            if (Path.IsPathRooted(configuredPath))
                return Path.GetFullPath(configuredPath);

            return Path.GetFullPath(Path.Combine(
                ServerPath.GetServersServerFiles(_serverData.ServerID),
                configuredPath));
        }

        private static string[] GetRptFiles(string directory)
        {
            var files = new List<string>();

            try
            {
                if (!Directory.Exists(directory))
                    return files.ToArray();

                try
                {
                    files.AddRange(Directory.GetFiles(directory, "*.rpt", SearchOption.TopDirectoryOnly));
                }
                catch
                {
                    // Continue with accessible child directories.
                }

                string[] childDirectories;
                try
                {
                    childDirectories = Directory.GetDirectories(directory);
                }
                catch
                {
                    childDirectories = new string[0];
                }

                foreach (string childDirectory in childDirectories)
                {
                    try
                    {
                        files.AddRange(Directory.GetFiles(childDirectory, "*.rpt", SearchOption.TopDirectoryOnly));
                    }
                    catch
                    {
                        // Ignore inaccessible profile subdirectories.
                    }
                }
            }
            catch
            {
                // Return whatever was discovered so far.
            }

            return files.ToArray();
        }

        private static Dictionary<string, long> SnapshotRptFiles(string directory)
        {
            var result = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

            try
            {
                if (!Directory.Exists(directory))
                    return result;

                foreach (string file in GetRptFiles(directory))
                {
                    try
                    {
                        result[file] = new FileInfo(file).Length;
                    }
                    catch
                    {
                        // Ignore files that disappear while taking the snapshot.
                    }
                }
            }
            catch
            {
                // The server itself must still be allowed to start if log discovery fails.
            }

            return result;
        }

        private static bool LooksLikeArmaRpt(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            return !string.IsNullOrWhiteSpace(fileName) &&
                   fileName.IndexOf("arma3", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string FindActiveRptFile(
            string directory,
            Dictionary<string, long> snapshot)
        {
            try
            {
                if (!Directory.Exists(directory))
                    return null;

                string newestPath = null;
                DateTime newestWrite = DateTime.MinValue;

                foreach (string file in GetRptFiles(directory))
                {
                    if (!LooksLikeArmaRpt(file))
                        continue;

                    try
                    {
                        var info = new FileInfo(file);
                        long previousLength;
                        bool existedBefore = snapshot.TryGetValue(file, out previousLength);
                        bool isNewOrChanged = !existedBefore || info.Length != previousLength;

                        if (!isNewOrChanged)
                            continue;

                        if (newestPath == null || info.LastWriteTimeUtc > newestWrite)
                        {
                            newestPath = file;
                            newestWrite = info.LastWriteTimeUtc;
                        }
                    }
                    catch
                    {
                        // Retry on the next polling pass.
                    }
                }

                return newestPath;
            }
            catch
            {
                return null;
            }
        }

        private void AddEmbeddedConsoleLine(ServerConsole serverConsole, string line)
        {
            if (serverConsole == null || line == null)
                return;

            try
            {
                if (DataReceivedEventArgsConstructor != null)
                {
                    var args = (DataReceivedEventArgs)DataReceivedEventArgsConstructor.Invoke(new object[] { line });
                    serverConsole.AddOutput(null, args);
                    return;
                }

                int serverId;
                if (int.TryParse(Convert.ToString(_serverData.ServerID), out serverId) &&
                    WindowsGSM.MainWindow._serverMetadata.ContainsKey(serverId))
                {
                    WindowsGSM.MainWindow._serverMetadata[serverId].ServerConsole.Add(line);
                }
            }
            catch
            {
                // Console mirroring must never interfere with the game server process.
            }
        }

        private async Task FollowRptLog(
            Process process,
            string rptDirectory,
            Dictionary<string, long> snapshot,
            ServerConsole serverConsole)
        {
            string rptFile = null;
            DateTime nextWaitingNotice = DateTime.UtcNow.AddSeconds(15);
            bool waitingNoticeShown = false;

            AddEmbeddedConsoleLine(serverConsole, "[WindowsGSM] Waiting for Arma 3 RPT output...");

            while (rptFile == null)
            {
                rptFile = FindActiveRptFile(rptDirectory, snapshot);
                if (rptFile != null)
                    break;

                try
                {
                    if (process.HasExited)
                    {
                        // One final lookup catches an RPT file written during process shutdown.
                        rptFile = FindActiveRptFile(rptDirectory, snapshot);
                        break;
                    }
                }
                catch
                {
                    break;
                }

                if (!waitingNoticeShown && DateTime.UtcNow >= nextWaitingNotice)
                {
                    waitingNoticeShown = true;
                    AddEmbeddedConsoleLine(
                        serverConsole,
                        "[WindowsGSM] Still waiting for an RPT file. Check -profiles= and make sure -noLogs is not enabled.");
                }

                await Task.Delay(250);
            }

            if (rptFile == null)
            {
                AddEmbeddedConsoleLine(serverConsole, "[WindowsGSM] No Arma 3 RPT file became available.");
                return;
            }

            long startOffset = 0;
            long oldLength;
            if (snapshot.TryGetValue(rptFile, out oldLength))
                startOffset = oldLength;

            FileStream stream = null;

            while (stream == null)
            {
                try
                {
                    stream = new FileStream(
                        rptFile,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite | FileShare.Delete);
                }
                catch (IOException)
                {
                    try
                    {
                        if (process.HasExited)
                            return;
                    }
                    catch
                    {
                        return;
                    }

                    await Task.Delay(200);
                }
                catch (UnauthorizedAccessException)
                {
                    AddEmbeddedConsoleLine(serverConsole, "[WindowsGSM] RPT file found, but it cannot be read (access denied).");
                    return;
                }
                catch
                {
                    return;
                }
            }

            using (stream)
            {
                if (startOffset > 0 && startOffset <= stream.Length)
                    stream.Seek(startOffset, SeekOrigin.Begin);

                using (var reader = new StreamReader(stream, Encoding.UTF8, true))
                {
                    AddEmbeddedConsoleLine(
                        serverConsole,
                        "[WindowsGSM] Reading " + Path.GetFileName(rptFile));

                    while (true)
                    {
                        string line = reader.ReadLine();
                        if (line != null)
                        {
                            AddEmbeddedConsoleLine(serverConsole, line);
                            continue;
                        }

                        bool exited;
                        try
                        {
                            exited = process.HasExited;
                        }
                        catch
                        {
                            exited = true;
                        }

                        if (exited)
                        {
                            // Give Arma a short moment to flush the last RPT lines.
                            await Task.Delay(300);
                            line = reader.ReadLine();
                            if (line == null)
                                break;

                            AddEmbeddedConsoleLine(serverConsole, line);
                            continue;
                        }

                        await Task.Delay(150);
                    }
                }
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

            string commandLine = param.ToString();
            string rptDirectory = null;
            Dictionary<string, long> rptSnapshot = null;
            ServerConsole serverConsole = null;
            bool rptConsoleEnabled = AllowsEmbedConsole && !HasStartSwitch(commandLine, "noLogs");

            if (AllowsEmbedConsole)
                serverConsole = new ServerConsole(_serverData.ServerID);

            if (rptConsoleEnabled)
            {
                try
                {
                    rptDirectory = GetRptDirectory(commandLine);
                    rptSnapshot = SnapshotRptFiles(rptDirectory);
                }
                catch
                {
                    rptConsoleEnabled = false;
                }
            }

            var p = new Process
            {
                StartInfo =
                {
                    WindowStyle = ProcessWindowStyle.Minimized,
                    UseShellExecute = false,
                    WorkingDirectory = ServerPath.GetServersServerFiles(_serverData.ServerID),
                    FileName = exePath,
                    Arguments = commandLine
                },
                EnableRaisingEvents = true
            };

            try
            {
                p.Start();

                if (AllowsEmbedConsole)
                {
                    if (HasStartSwitch(commandLine, "noLogs"))
                    {
                        AddEmbeddedConsoleLine(
                            serverConsole,
                            "[WindowsGSM] Embedded console cannot mirror Arma output because -noLogs disables the RPT log.");
                    }
                    else if (!rptConsoleEnabled || string.IsNullOrWhiteSpace(rptDirectory))
                    {
                        AddEmbeddedConsoleLine(
                            serverConsole,
                            "[WindowsGSM] Embedded console could not determine the Arma RPT directory.");
                    }
                    else
                    {
#pragma warning disable 4014
                        Task.Run(() => FollowRptLog(p, rptDirectory, rptSnapshot, serverConsole));
#pragma warning restore 4014
                    }
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
