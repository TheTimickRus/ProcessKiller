using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using ProcessKiller.Classes;

namespace ProcessKiller
{
    public class MainClass
    {
        private static JObject _jObject;
        private static Logger _logger;

        private static List<string> _killList;
        private static int _timerInterval;

        private static int _counter;

        private static void CreateLogger() 
        {
            var lastLog = $"{AppDomain.CurrentDomain.BaseDirectory}\\log.lg";
            var logBackupsFolder = $"{AppDomain.CurrentDomain.BaseDirectory}\\LogBackups";

            try
            {
                if (File.Exists(lastLog))
                {
                    if (!Directory.Exists(logBackupsFolder))
                    {
                        Directory.CreateDirectory(logBackupsFolder);
                    }

                    var dateLastWrite = new FileInfo(lastLog).LastWriteTime;

                    File.Move(lastLog, $"{logBackupsFolder}\\Log ({dateLastWrite:dd MMMM yyyy (hh mm ss)}).lg");
                }
            }
            catch
            {
                // ignored
            }

            _logger = LogManager.GetCurrentClassLogger();

            var config = new NLog.Config.LoggingConfiguration();

            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = lastLog };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;

            _logger.Info("");
            _logger.Info("The launch is successful!");
            _logger.Info("");
        }
        private static void GetSetting() 
        {
            var file = $@"{AppDomain.CurrentDomain.BaseDirectory}\Settings.json";

            try
            {
                //***************************************

                _killList = new List<string>();

                var str = File.ReadAllText(file);
                _jObject = JObject.Parse(str);

                var processes = _jObject["Processes"].Value<JArray>();

                foreach (var process in processes)
                {
                    _killList.Add(process.ToString());
                }

                //***************************************

                if ((bool) _jObject["HideWindow"])
                {
                    ConsoleWindow.Hide();
                }

                //***************************************

                NativeMethods.BlockInput((bool)_jObject["BlockInput"]);

                //***************************************

                _timerInterval = (int) _jObject["Interval"];

                //***************************************

                var blockStream = new FileStream(file, FileMode.Open);

                //***************************************
            }
            catch (Exception ex)
            {
                _logger.Error($"{ex.Message}\n***********\n{ex.StackTrace}\n***********");
                Console.ReadKey();

                Environment.Exit(-1);
            }

        }
        public static string ConvertComputerValues(dynamic value)
        {
            string[] suf = { "Byte", "KB", "MB", "GB", "TB", "PB", "EB" };

            if (value == 0) return "0" + suf[0];

            var bytes = Math.Abs(value);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);

            return Math.Sign(value) * num + " " + suf[place];
        }

        public static void Main()
        {
            CreateLogger();
            _counter = 0;
            
            _logger.Info("Reading settings");

            GetSetting();

            _logger.Info("Reading settings finished!");
            _logger.Info("");


            _logger.Info("Creating timer");

            var timer = new System.Timers.Timer(_timerInterval);
            timer.Elapsed += TimerOnElapsed;
            timer.Start();

            _logger.Info("Create timer finished!");
            _logger.Info("");
            _logger.Info("");
            _logger.Info("");

            TimerOnElapsed(null, null);

            Console.ReadKey();
        }

        private static void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _logger.Info("=== Method run! === ");
            _logger.Info("Number of launches: " + _counter);

            _counter++;

            try
            {
                var getProcesses = Process.GetProcesses().ToList();

                _logger.Info("-------------------------");
                _logger.Info("Number of processes: " + getProcesses.Count);
                _logger.Info("Kill processes:");

                getProcesses.ForEach(delegate(Process process)
                {
                     _killList.ForEach(delegate(string killProcess)
                     {
                         if (string.Equals(process.ProcessName, killProcess, StringComparison.CurrentCultureIgnoreCase))
                         {
                             process.Kill();
                             _logger.Info($"{process.ProcessName} (PID = {process.Id}, Memory = {ConvertComputerValues(process.PrivateMemorySize64)})");
                         }
                     });
                });

                _logger.Info("-------------------------");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }

            _logger.Info("=== The method is completed! === ");
            _logger.Info("");
            _logger.Info("");
            _logger.Info("");
        }
    }

}
