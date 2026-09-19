using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;

namespace SAI_WAS_HERE
{
    internal class Program
    {
        private static string globalDirectory = "";
        private static string currentPath = Directory.GetCurrentDirectory();
        private static readonly CancellationTokenSource cts = new CancellationTokenSource();
        private static string customAppsJsonPath = null;

        [STAThread]
        static async Task Main(string[] args)
        {
            // Clean steam environment before everything.
            // WE LEAVE THIS MANDATORY HERE DON'T MOVE OR DELETE.
            SteamDetach.RemoveSteamEnvironments();

            Console.Title = "SAI_WAS_HERE V1.6.8.1-hotfix1";

            for (int i = 0; i < args.Length; i++)
            {
                if ((args[i] == "--apps-json" || args[i] == "-a") && i + 1 < args.Length)
                {
                    customAppsJsonPath = args[i + 1]; i++;
                }
            }

            Console.WriteLine("SAI_WAS_HERE V1.6.8.1-hotfix1");
            Console.WriteLine("");

            if (!Directory.Exists(@"C:\Asgard"))
            {
                Console.WriteLine("[!] Not a GeForce NOW environment. Exiting...");
                await Task.Delay(5000); Environment.Exit(0);
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, errors) => true;
            ServicePointManager.DefaultConnectionLimit = 32;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;

            // Recovery mode prompt
            const string text = "Press DEL key for recovery mode";
            DateTime start = DateTime.Now;
            DateTime end = start.AddSeconds(1.5);

            while (DateTime.Now < end)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.Delete)
                    {
                        // Clear the prompt line
                        Console.Write("\r" + new string(' ', Console.BufferWidth - 1) + "\r");

                        Console.WriteLine("Recovery mode selected.");
                        await RecoveryMode.ShowRecoveryPrompt();
                        return;
                    }
                }

                int dots = Math.Min((int)(DateTime.Now - start).TotalSeconds + 1, 3);

                Console.Write($"\r{text}{new string('.', dots)}");

                Thread.Sleep(10);
            }

            // Clear the prompt line before continuing
            Console.Write("\r" + new string(' ', Console.BufferWidth - 1) + "\r");

            await Startup();

            // Load configuration once to share settings across modules
            AppSettings.Load(globalDirectory);

            _ = Task.Run(() => BackgroundTasks.EnvironmentSetup());

            // Apply registry changes and backup desktop registry
            _ = AutoPersist.BackupDesktopRegistry(cts.Token, globalDirectory);
            _ = AutoPersist.ApplyCustomRegistryFiles(globalDirectory);
            _ = AutoPersist.SetupGameSavesAsync(globalDirectory);

            // Fire and forget non-blocking background services
            _ = BackgroundTasks.StartShortcutsSavingAsync(globalDirectory, cts.Token);
            _ = BackgroundTasks.StartTerminateGFNExplorerShellAsync(cts.Token);
            _ = BackgroundTasks.StartEacWatcherAsync(cts.Token);
            _ = BackgroundTasks.StartBrickPreventionAsync(cts.Token);
            _ = DotNetInstaller.StartDotNetInstallAsync(cts.Token);
            _ = Task.Run(() => NvidiaManager.EnableRTX());

            await Task.WhenAll(
                SteamManager.ShutdownServerAsync(globalDirectory),
                DesktopInstaller.DesktopInstallAsync(globalDirectory),
                AppInstaller.AppsInstallAsync(globalDirectory, customAppsJsonPath),
                AppInstaller.AppsInstallSilentAsync(globalDirectory)
            );

            NativeMethods.ShowWindow(NativeMethods.GetConsoleWindow(), NativeMethods.SW_HIDE);

            await FinalBackgroundTasks.OpenShellStartup(globalDirectory);

            try { await Task.Delay(Timeout.Infinite, cts.Token); } catch (TaskCanceledException) { }
        }

        static async Task Startup()
        {
            try
            {
                using (var wc = new WebClient())
                {
                    var dir = JsonConvert.DeserializeObject<System.Collections.Generic.List<SavePath>>(await wc.DownloadStringTaskAsync("https://raw.githubusercontent.com/artem-sobolevskyi/gfn-assets/main/jsons/directory.json"))[0];
                    globalDirectory = dir.directoryCreate;
                    Directory.CreateDirectory(globalDirectory);
                    
                    // Initialize Logger here so it knows the global directory path
                    AppLogger.Initialize(globalDirectory);
                    AppLogger.Info($"Main directory created {globalDirectory}");
                    
                    string cfg = Path.Combine(globalDirectory, "SAI_WAS_HEREConfig.ini");
                    if (!System.IO.File.Exists(cfg)) await wc.DownloadFileTaskAsync(new Uri("https://raw.githubusercontent.com/artem-sobolevskyi/gfn-assets/main/jsons/SAI_WAS_HEREConfig.ini"), cfg);
                }
            }
            // Upload Crashlogs to paste.rs and show the user a link to forward to the Devs
            catch (Exception ex) 
            { 
                AppLogger.UploadLogAndShowError(ex.Message);
                Environment.Exit(0);
            }
        }
    }
}