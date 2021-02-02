using System;
using System.Runtime.InteropServices;

namespace ProcessKiller.Classes
{
    public class ConsoleWindow
    {
        private const int SwHide = 0;
        private const int SwShow = 5;
        private const int SwMin = 2;
        private const int SwMax = 3;
        private const int SwNorm = 4;

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static bool Hide()
        {
           return ShowWindow(GetConsoleWindow(), SwHide);
        }

        public static bool Show()
        {
            return ShowWindow(GetConsoleWindow(), SwShow);
        }
    }
}
