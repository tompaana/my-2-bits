using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMonitorConsole
{
    public class Logger
    {
        public static void Log(string message, ConsoleColor consoleColor)
        {
            Console.ForegroundColor = consoleColor;
            LogToConsole(message);
            Console.ResetColor();
        }

        public static void Log(string message)
        {
            Console.ResetColor();
            LogToConsole(message);
        }

        public static void LogError(string errorMessage)
        {
            Log(errorMessage, ConsoleColor.Red);
        }

        private static void LogToConsole(string message)
        {
            Console.WriteLine(string.Format("[{0:HH:mm:ss}] ", DateTime.Now) + message);
        }
    }
}
