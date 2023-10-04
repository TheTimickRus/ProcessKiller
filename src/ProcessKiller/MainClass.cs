using Newtonsoft.Json;
using ProcessKiller.Logger;
using ProcessKiller.Settings;
using ProcessKiller.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;

// ReSharper disable ClassNeverInstantiated.Global

namespace ProcessKiller;

public class MainClass
{
    #region PrivateProps

    private static int _counter;
    private static AppSettings? _settings;

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

    private static void TimerOnElapsed(object? sender, ElapsedEventArgs? e)
    {
        MyLogger.Instance.Information("");
        MyLogger.Instance.Information("=== Method run! === ");
        MyLogger.Instance.Information("Number of launches: {Counter}", _counter);

        _counter++;

        try
        {
            var getProcesses = Process.GetProcesses().ToList();

            MyLogger.Instance.Information("-------------------------");
            MyLogger.Instance.Information("Number of processes: {Count}", getProcesses.Count);
            MyLogger.Instance.Information("Kill processes:");

            if (_settings is null) return;
            
            foreach (var proc in _settings.Processes)
            {
                var procFromSystem = getProcesses.FirstOrDefault(process => process.ProcessName == proc.Name);
                if (procFromSystem is null) continue;

                var skipFactorFlag = proc
                    .SkipFactors
                    .Select(skipFactor => getProcesses.FirstOrDefault(process => process.ProcessName == skipFactor))
                    .Any(sfSystem => sfSystem is not null);
                if (skipFactorFlag)  continue;

                procFromSystem.Kill();
                    
                MyLogger.Instance.Information(
                    "{Name} (PID = {Id}, Memory = {ConvertComputerValues})", 
                    procFromSystem.ProcessName, 
                    procFromSystem.Id, 
                    Utils.Utils.ConvertComputerValues(procFromSystem.PrivateMemorySize64)
                );
            }
                
            MyLogger.Instance.Information("-------------------------");
        }
        catch (Exception ex)
        {
            MyLogger.Instance.Error("{ErrorMessage}", ex.Message);
        }

        MyLogger.Instance.Information("=== The method is completed! === ");
    }

    #endregion
}