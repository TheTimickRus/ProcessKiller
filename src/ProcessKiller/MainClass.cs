using Newtonsoft.Json;
using ProcessKiller.Logger;
using ProcessKiller.Settings;
using ProcessKiller.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;

namespace ProcessKiller
{
    public class MainClass
    {
        #region PrivateProps

        private static int _counter;
        private static AppSettings _settings;

        #endregion

        public static void Main()
        {
            try
            {
                Variables.Init();
                
                MyLogger.Instance.Information("Reading settings");
                
                _settings = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(Variables.SettingsFile));

                if (_settings is null)
                    throw new Exception("_settings is null!");

                if (_settings.HideWindow)
                    ConsoleWindow.Hide();

                MyLogger.Instance.Information("Reading settings finished!");
                MyLogger.Instance.Information("Creating timer");

                var timer = new Timer(_settings.Interval);
                timer.Elapsed += TimerOnElapsed;
                timer.Start();

                MyLogger.Instance.Information("Create timer finished!");

                if (_counter == 0)
                    TimerOnElapsed(null, null);
            }
            catch (Exception ex)
            {
                MyLogger.Instance.Fatal(ex, "Error!");
                return;
            }

            Console.ReadKey();
            MyLogger.Instance.Information("Program closed!");
        }

        #region Methods

        private static void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            MyLogger.Instance.Information("");
            MyLogger.Instance.Information("=== Method run! === ");
            MyLogger.Instance.Information("Number of launches: " + _counter);

            _counter++;

            try
            {
                var getProcesses = Process.GetProcesses().ToList();

                MyLogger.Instance.Information("-------------------------");
                MyLogger.Instance.Information("Number of processes: " + getProcesses.Count);
                MyLogger.Instance.Information("Kill processes:");

                getProcesses.ForEach(processFromSystem =>
                {
                    _settings.Processes.ToList().ForEach(processNameFromKillList =>
                    {
                        if (string.Equals(processFromSystem.ProcessName, processNameFromKillList, StringComparison.CurrentCultureIgnoreCase))
                        {
                            processFromSystem.Kill();
                            MyLogger.Instance.Information(
                                $"{processFromSystem.ProcessName} (PID = {processFromSystem.Id}, Memory = {Utils.Utils.ConvertComputerValues(processFromSystem.PrivateMemorySize64)})");
                        }
                    });
                });
                
                MyLogger.Instance.Information("-------------------------");
            }
            catch (Exception ex)
            {
                MyLogger.Instance.Error(ex.Message);
            }

            MyLogger.Instance.Information("=== The method is completed! === ");
        }

        #endregion
    }
}
