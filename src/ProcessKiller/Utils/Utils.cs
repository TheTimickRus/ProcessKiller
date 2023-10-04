using System;

namespace ProcessKiller.Utils;

internal static class Utils
{
    public static string ConvertComputerValues(dynamic value)
    {
        string[] suf = { "Byte", "KB", "MB", "GB", "TB", "PB", "EB" };

        if (value == 0) return "0" + suf[0];

        var bytes = Math.Abs(value);
        var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        var num = Math.Round(bytes / Math.Pow(1024, place), 1);

        return Math.Sign(value) * num + " " + suf[place];
    }
}