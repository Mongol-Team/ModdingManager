using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerClassLib
{
    public class Debugger
    {
        private static Debugger _instance;
        public static Debugger Instance => _instance ??= new Debugger();

        /// <summary>Обработчик для обновления UI (настраивается снаружи)</summary>
        public Action<string> OutputHandler { get; set; }

        public string Log { get; private set; } = string.Empty;

        private Debugger()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        public void LogMessage(string message)
        {
            Log += $"[{DateTime.Now:HH:mm:ss.fff}] {message}\n";
            OutputHandler?.Invoke(Log);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogMessage($"UNHANDLED EXCEPTION: {e.ExceptionObject}");
        }


    }
}

