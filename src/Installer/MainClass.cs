using Microsoft.Win32.TaskScheduler;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

// ReSharper disable ClassNeverInstantiated.Global

namespace Installer
{
    public class Settings
    {
        public bool SilentInstall { get; set; }
        public string FolderName { get; set; } = "uts";
        public string FileName { get; set; } = "Updater";
        public string TaskName { get; set; } = "UTS";
    }

    public class MainClass
    {
        private static Settings? _settings;

        private static Settings? GetSetting(string file = "InstallerSettings.json")
        {
            try
            {
                return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(file));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private static void SaveSettings(Settings settings)
        {
            try
            {
                File.WriteAllText("InstallerSettings.json", JsonConvert.SerializeObject(settings, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static Process? FindProcess(string name)
        {
            return Process.GetProcesses().FirstOrDefault(proc => proc.ProcessName == name);
        }

        public static void Main()
        {
            _settings = GetSetting();
            if (_settings is null)
            {
                SaveSettings(
                    new Settings
                    {
                        SilentInstall = false,
                        FileName = "uts",
                        FolderName = "Updater",
                        TaskName = "UTS"
                    }
                );
                
                return;
            }
            
            if (_settings.SilentInstall)
            {
                Install();
                return;
            }

            Console.Write("Install/Uninstall (I/U): ");

            switch (Console.ReadLine())
            {
                case "I":
                    Install();
                    break;

                case "U":
                    Uninstall();
                    break;

                default:
                    Console.WriteLine("Unknown command!");
                    break;
            }

            Console.ReadKey();
        }

        private static void Install()
        {
            try
            {
                if (_settings is null) throw new Exception("_settings is null");
                
                var path = $"{Environment.CurrentDirectory}\\Files";
                var pathInstall = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{_settings.FolderName}";

                Console.WriteLine("\n******************");

                var process = FindProcess(_settings.FileName);
                if (process != null)
                {
                    Console.WriteLine("Kill process...");
                    process.Kill();
                    Thread.Sleep(1000);
                }

                Console.WriteLine("Create directory...");
                if (!Directory.Exists(pathInstall))
                {
                    Directory.CreateDirectory(pathInstall);
                }
                else
                {
                    Directory.Delete(pathInstall, true);
                    Directory.CreateDirectory(pathInstall);
                }

                Console.WriteLine("Copy files...");
                var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).ToList();
                files.ForEach(file =>
                {
                    File.Copy(file, Path.GetExtension(file) == ".exe"
                        ? $"{pathInstall}\\{_settings.FileName}.exe"
                        : $"{pathInstall}\\{Path.GetFileName(file)}");
                });

                Console.WriteLine("Create task...");
                using (var ts = new TaskService())
                {
                    var td = ts.NewTask();

                    td.Triggers.Add(new LogonTrigger());
                    td.Actions.Add(new ExecAction($"{pathInstall}\\{_settings.FileName}.exe", "", pathInstall));
                    td.Settings.Hidden = true;

                    ts.RootFolder.RegisterTaskDefinition(_settings.TaskName, td);

                    Console.WriteLine("Start task...");
                    ts.RootFolder.GetTasks()[_settings.TaskName].Run();
                }

                if (FindProcess(_settings.FileName) == null)
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = $"{pathInstall}\\{_settings.FileName}.exe",
                        WorkingDirectory = pathInstall
                    };

                    Process.Start(psi);
                }

                Console.WriteLine("******************\n\nInstall successful!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void Uninstall()
        {
            try
            {
                if (_settings is null) throw new Exception("_settings is null");
                
                var pathInstall = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{_settings.FolderName}";

                Console.WriteLine("\n******************");

                var process = FindProcess(_settings.FileName);
                if (process != null)
                {
                    Console.WriteLine("Kill process...");
                    process.Kill();
                    Thread.Sleep(1000);
                }

                Console.WriteLine("Delete directory...");
                if (Directory.Exists(pathInstall))
                {
                    Directory.Delete(pathInstall, true);
                }

                Console.WriteLine("Delete task...");
                using (var ts = new TaskService())
                {
                    ts.RootFolder.DeleteTask(_settings.TaskName, false);
                }

                Console.WriteLine("******************\n\nUninstall successful!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
