using System.Collections.Generic;

namespace ProcessKiller.Settings;

public class AppSettings
{
    public record ProcessEntity
    {
        public string Name { get; set; } = "";
        public IEnumerable<string> SkipFactors { get; set; } = new List<string>();
    }
    
    public bool HideWindow { get; set; } = false;
    public int Interval { get; set; } = 10000;
    public IEnumerable<ProcessEntity> Processes { get; set; } = new List<ProcessEntity>
    {
        new()
    };
}