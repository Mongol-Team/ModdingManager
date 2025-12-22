using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;  // Для Debug.WriteLine в fallback і діагностиці

namespace ModdingManagerClassLib.Debugging
{
    public class LogLevel
    {
        public static readonly LogLevel Info = new LogLevel(3, "INF", ConsoleColor.White);
        public static readonly LogLevel Warning = new LogLevel(2, "WAR", ConsoleColor.Yellow);
        public static readonly LogLevel Error = new LogLevel(1, "ERR", ConsoleColor.Red);

        private readonly int _value;
        private readonly string _typeString;
        private readonly ConsoleColor _color;

        private LogLevel(int value, string typeString, ConsoleColor color)
        {
            _value = value;
            _typeString = typeString;
            _color = color;
        }

        public int GetValue() => _value;
        public string GetTypeString() => _typeString;
        public ConsoleColor GetColor() => _color;

        public bool IsError() => this == Error;
        public bool ShouldLog(int loggingLevel) => _value <= loggingLevel;
    }

    public struct Logger
    {
        private static readonly object _logLock = new object();
        public static bool IsDebug = false;
        private class LogEntry
        {
            public string Message { get; set; } = "";
            public ConsoleColor Color { get; set; }
            public LogLevel LogLevel { get; set; }
            public string MessageType { get; set; } = "UNK";
            public string Caller { get; set; } = "";
            public string File { get; set; } = "";
            public int Line { get; set; }
            public DateTime Time { get; set; }
            public int Depth { get; set; }
            public int ThreadId { get; set; }
        }

        private static readonly List<LogEntry> _buffer = new List<LogEntry>();

        public static int LoggingLevel { get; set; } = 3; // Змінено дефолт на 3 ("all"), щоб логи завжди йшли за замовчуванням
        public static int depth = 0;
        public const ConsoleColor _DEFAULT_OUTLINE_COLOR = ConsoleColor.Cyan;
        public static ConsoleColor outline_color = ConsoleColor.Cyan;
        public static string WarnSymbol = "⚠️";
        public static string ErrorSymbol = "❌";

        /// <summary>
        /// Цель для отладочных логов. Если не null, AddDbgLog игнорирует все логи, кроме тех, где dbgSource совпадает с этим значением.
        /// </summary>
        public static string DbgTarget { get; set; } = null;

        private static bool HasConsole =>
            Console.OpenStandardOutput(1) != Stream.Null;

        public static void FlushBuffer()
        {
            lock (_logLock)
            {
                foreach (var entry in _buffer)
                    PrintToConsole(entry);

                _buffer.Clear();
            }
        }

        private static void PrintToConsole(LogEntry entry)
        {
            string time = entry.Time.ToString("yyyy-MM-dd HH:mm:ss");
            string offset = "";

            for (int q = 0; q < entry.Depth; q++)
                offset += " |";
            if (entry.Depth > 0)
                offset += "->";

            string logLine = $"[{time}]{offset}{entry.Message} [{entry.MessageType}][{entry.ThreadId}][{entry.Caller} @ {entry.File}:{entry.Line}]";

            if (HasConsole)
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"[{time}]");
                    Console.ForegroundColor = outline_color;
                    Console.Write($"{offset}");
                    Console.ForegroundColor = entry.Color;
                    Console.Write($"{entry.Message} ");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"[{entry.MessageType}][{entry.ThreadId}][{entry.Caller} @ {entry.File}:{entry.Line}]");
                    Console.WriteLine();
                    Console.ResetColor();
                }
                catch (IOException ex)
                {
                    // Fallback на Debug.WriteLine при помилці
                    Debug.WriteLine(logLine + " (IOException in console output: " + ex.Message + ")");
                }
            }
            else
            {
                // Fallback для non-console apps
                Debug.WriteLine(logLine);
            }
        }

        public static async Task AddLog(
            string message,
            ConsoleColor color,
            int log_level = 1,
            string message_type = "UNK",
            [CallerMemberName] string caller = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            // Діагностика: вивід перед if, щоб побачити, чому не входить (видно в VS Output window)
            Debug.WriteLine($"[DIAG] AddLog called: LoggingLevel={LoggingLevel}, log_level={log_level}, shouldLog={LoggingLevel >= log_level}");

            if (LoggingLevel >= log_level)
            {
                // Діагностика: підтвердження входу в if
                Debug.WriteLine($"[DIAG] Entering log processing for message: {message}");

                string fileShort = Path.GetFileName(file);

                var entry = new LogEntry
                {
                    Message = message,
                    Color = color,
                    LogLevel = null,
                    MessageType = message_type,
                    Caller = caller,
                    File = fileShort,
                    Line = line,
                    Time = DateTime.Now,
                    Depth = depth,
                    ThreadId = Thread.CurrentThread.ManagedThreadId
                };

                lock (_logLock)
                {
                    PrintToConsole(entry);
                    _buffer.Add(entry);
                }
            }
            else
            {
                // Діагностика: чому пропустили
                Debug.WriteLine($"[DIAG] Skipped log: LoggingLevel {LoggingLevel} < log_level {log_level}");
            }
        }

        public static async Task AddLog(
            string message,
            LogLevel msgType = null,
            [CallerMemberName] string caller = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            msgType ??= LogLevel.Info;

            ConsoleColor color = msgType.GetColor();
            string type = msgType.GetTypeString();
            int level = msgType.GetValue();

            if (message.Contains(WarnSymbol))
            {
                color = ConsoleColor.Yellow;
                type = "WAR";
            }
            if (message.Contains(ErrorSymbol))
            {
                color = ConsoleColor.Red;
                type = "ERR";
            }
            await AddLog(message, color, level, type, caller, file, line);
        }

        /// <summary>
        /// Добавляет отладочный лог, но только если IsDebug=true и имя отправителя совпадает с DbgTarget.
        /// </summary>
        public static async Task AddDbgLog(
            string message,
            string dbgSource = null,  // По умолчанию null, если не передан - игнорируем фильтрацию
            LogLevel msgType = null,
            [CallerMemberName] string caller = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            if (!IsDebug)
            {
                return;
            }

            if (DbgTarget != null && (dbgSource == null || dbgSource != DbgTarget))
            {
                return;
            }

            msgType ??= LogLevel.Info;

            ConsoleColor color = msgType.GetColor();
            string type = msgType.GetTypeString();
            int level = msgType.GetValue();

            if (message.Contains(WarnSymbol))
            {
                color = ConsoleColor.Yellow;
                type = "WAR";
            }
            if (message.Contains(ErrorSymbol))
            {
                color = ConsoleColor.Red;
                type = "ERR";
            }
            await AddLog(message, color, level, type, caller, file, line);
        }

    }
}