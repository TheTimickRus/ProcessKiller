using System;
using System.IO;
using Newtonsoft.Json;
using ProcessKiller.Logger;
using ProcessKiller.Settings;

namespace ProcessKiller
{
    public static class Variables
    {
        public static string LogsFolder => Path.Combine(Environment.CurrentDirectory, "Logs");
        public static string SettingsFile => Path.Combine(Environment.CurrentDirectory, "Settings.json");

        public static void Init()
        {
            if (Directory.Exists(LogsFolder) is false) 
                Directory.CreateDirectory(LogsFolder);

            if (File.Exists(SettingsFile) is false || Deserialize() is false)
            {
                try
                {
                    File.WriteAllText(SettingsFile, JsonConvert.SerializeObject(new AppSettings(), Formatting.Indented));
                }
                catch (Exception ex)
                {
                    MyLogger.Instance.Fatal(ex, "Error creating the settings file!");
                }
            }
        }

        private static bool Deserialize()
        {
            try
            {
                JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(SettingsFile));
            }
            catch
            {
                File.Delete(SettingsFile);
                return false;
            }

            return true;
        }
    }
}
