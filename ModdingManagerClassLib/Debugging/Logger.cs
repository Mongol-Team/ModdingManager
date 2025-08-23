
using System.Runtime.CompilerServices;

namespace ModdingManagerClassLib.Debugging
{
    public enum LogLevel
    {
        INFO,
        ERROR,
        WARNING,
    }

    public struct Logger
    {
        private static readonly object _logLock = new object();
        public static int LoggingLevel { get; set; } = 0; // 0 - no logging, 1 - only error, 2 - warnings, 3 - all
        public static int depth = 0;
        public const ConsoleColor _DEFAULT_OUTLINE_COLOR = ConsoleColor.Cyan;
        public static ConsoleColor outline_color = ConsoleColor.Cyan;
        public static string WarnSymbol = "⚠️";
        public static string ErrorSymbol = "❌";
        public static async Task AddLog(
            string message,
            ConsoleColor color,
            int log_level = 1,
            string message_type = "UNK",
            [CallerMemberName] string caller = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            await Task.Yield();


            if (LoggingLevel >= log_level)
            {
                string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string fileShort = System.IO.Path.GetFileName(file);

                string offset = "";

                for (int q = 0; q < depth; q++)
                    offset += " |";
                if (depth > 0)
                    offset += "->";
                else if (color == ConsoleColor.White)
                    color = outline_color;


                int thread_id = Thread.CurrentThread.ManagedThreadId;

                if (message.Contains(WarnSymbol))
                    color = ConsoleColor.Yellow;
                if (message.Contains(ErrorSymbol))
                    color = ConsoleColor.Red;

                lock (_logLock)
                {
                                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"[{time}]");
                    Console.ForegroundColor = outline_color;
                    Console.Write($"{offset}");
                    Console.ForegroundColor = color;
                    Console.Write($"{message} ");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"[{message_type}][{thread_id}][{caller} @ {fileShort}:{line}]");
                    Console.WriteLine();
                    Console.ResetColor();
                }
            }
        }
        public static async Task AddLog(
        string message,
        LogLevel msgType = LogLevel.INFO,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
        {
            ConsoleColor color = msgType == LogLevel.ERROR ? ConsoleColor.Red : msgType == LogLevel.WARNING ? ConsoleColor.Yellow : ConsoleColor.White;
            string type = msgType == LogLevel.ERROR ? "ERR" : msgType == LogLevel.WARNING ? "WAR" : "INF";
            int level = msgType == LogLevel.ERROR ? 1 : msgType == LogLevel.WARNING ? 2 : 3;
            await AddLog(message, color, level, type, caller, file, line);
        }

     
    }
}
