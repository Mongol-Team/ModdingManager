using ModdingManager.managers.utils;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

public class Debugger
{
    private static Debugger _instance;
    public static Debugger Instance => _instance ??= new Debugger();

    public object DebugOutputControl { get; set; }
    public string Log { get; private set; } = string.Empty;

    private Debugger()
    {
        // Глобальные обработчики для WinForms
        System.Windows.Forms.Application.ThreadException += OnWinFormsThreadException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }
    public static void AttachIfDebug(Window thisW)
    {
        if (ModManager.IsDebugRuning)
        {
            Debugger.Instance.AttachToWindow<Window>(thisW);
        }
    }
    /// <summary>
    /// Подключает обработчик исключений для конкретного окна
    /// </summary>
    public void AttachToWindow<TWindow>(TWindow window) where TWindow : class
    {
        switch (window)
        {
            case Window wpfWindow:
                wpfWindow.Dispatcher.UnhandledException += OnWpfException;
                break;
            case Form winFormsWindow:
                // Для WinForms используем глобальные обработчики
                break;
            default:
                throw new ArgumentException("Unsupported window type");
        }
    }

    public void LogMessage(string message)
    {
        Log += $"[{DateTime.Now:HH:mm:ss.fff}] {message}\n";
        UpdateDebugOutput();
    }

    private void UpdateDebugOutput()
    {
        if (DebugOutputControl == null) return;

        switch (DebugOutputControl)
        {
            case System.Windows.Controls.RichTextBox wpfRichTextBox:
                wpfRichTextBox.Dispatcher.Invoke(() =>
                {
                    wpfRichTextBox.Document.Blocks.Clear();
                    wpfRichTextBox.Document.Blocks.Add(new System.Windows.Documents.Paragraph(
                        new System.Windows.Documents.Run(Log)));
                    wpfRichTextBox.ScrollToEnd();
                });
                break;
            case RichTextBox winFormsRichTextBox:
                if (winFormsRichTextBox.InvokeRequired)
                {
                    winFormsRichTextBox.Invoke((Action)(() =>
                    {
                        winFormsRichTextBox.Text = Log;
                        winFormsRichTextBox.SelectionStart = winFormsRichTextBox.Text.Length;
                        winFormsRichTextBox.ScrollToCaret();
                    }));
                }
                else
                {
                    winFormsRichTextBox.Text = Log;
                    winFormsRichTextBox.SelectionStart = winFormsRichTextBox.Text.Length;
                    winFormsRichTextBox.ScrollToCaret();
                }
                break;
        }
    }

    // Обработчики исключений
    private void OnWpfException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogMessage($"WPF EXCEPTION: {e.Exception}");
        e.Handled = true;
    }

    private void OnWinFormsThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
    {
        LogMessage($"WinForms THREAD EXCEPTION: {e.Exception}");
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LogMessage($"UNHANDLED EXCEPTION: {e.ExceptionObject}");
    }
}