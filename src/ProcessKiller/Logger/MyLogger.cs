using System;
using System.IO;
using Serilog;

namespace ProcessKiller.Logger
{
    public class MyLogger
    {
        private static Serilog.Core.Logger _instance;
        public static Serilog.Core.Logger Instance => _instance ??= GetLogger();

        private static Serilog.Core.Logger GetLogger()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "Logs");

            if (Directory.Exists(path) is false)
            {
                Directory.CreateDirectory(path);
            }

            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(Path.Combine(path, $"MyLogger ({DateTime.Now.ToString("G").Replace(':', '-')}).lg"))
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}
