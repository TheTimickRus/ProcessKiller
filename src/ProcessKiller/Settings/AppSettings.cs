using System.Collections.Generic;

namespace ProcessKiller.Settings
{
    public class AppSettings
    {
        public bool HideWindow { get; set; } = false;
        public int Interval { get; set; } = 10000;
        public IList<string> Processes { get; set; } = new List<string> { "Steam" };
    }
}
