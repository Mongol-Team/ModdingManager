using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace View.Controls
{
    /// <summary>
    /// Логика взаимодействия для DebugControl.xaml
    /// </summary>
    public partial class DebugControl : UserControl
    {
        private const int MAX_LINES = 1000; // Ограничиваем количество строк
        private bool _isInitialized = false;

        public DebugControl()
        {
            InitializeComponent();
            // Отложенная загрузка логов
            Loaded += OnDebugControlLoaded;
        }

        private void OnDebugControlLoaded(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                // Запускаем загрузку в фоновом потоке
                Task.Run(() => LoadLogsFromLoggerAsync());
            }
        }

        private async Task LoadLogsFromLoggerAsync()
        {
            try
            {
                var logs = Application.Debugging.Logger.GetBufferedLogs().ToList();

                // Разбиваем на порции для постепенной загрузки
                const int BATCH_SIZE = 50;
                for (int i = 0; i < logs.Count; i += BATCH_SIZE)
                {
                    var batch = logs.Skip(i).Take(BATCH_SIZE).ToList();

                    await Dispatcher.InvokeAsync(() =>
                    {
                        foreach (var (message, color) in batch)
                        {
                            AddLogFast(message, ConvertColor(color));
                        }

                        // Даем UI обновиться
                        System.Windows.Application.Current.Dispatcher.Invoke(() => { },
                            System.Windows.Threading.DispatcherPriority.Background);
                    });

                    // Маленькая задержка между порциями
                    if (i + BATCH_SIZE < logs.Count)
                    {
                        await Task.Delay(10);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading logs: {ex.Message}");
            }
        }

        private Brush ConvertColor(ConsoleColor consoleColor)
        {
            return consoleColor switch
            {
                ConsoleColor.White => Brushes.White,
                ConsoleColor.Yellow => Brushes.Yellow,
                ConsoleColor.Red => Brushes.Red,
                ConsoleColor.Cyan => Brushes.Cyan,
                ConsoleColor.DarkGray => Brushes.DarkGray,
                _ => Brushes.White
            };
        }

        /// <summary>
        /// Быстрое добавление строки лога (без ScrollToEnd)
        /// </summary>
        private void AddLogFast(string message, Brush? color = null)
        {
            try
            {
                // Используем TextRange для более быстрого добавления
                var paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run(message)
                {
                    Foreground = color ?? Brushes.White
                });

                DebugBox.Document.Blocks.Add(paragraph);

                // Ограничиваем количество строк
                if (DebugBox.Document.Blocks.Count > MAX_LINES)
                {
                    DebugBox.Document.Blocks.Remove(DebugBox.Document.Blocks.FirstBlock);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding log: {ex.Message}");
            }
        }

        /// <summary>
        /// Добавить строку лога (публичный метод)
        /// </summary>
        public void AddLog(string message, Brush? color = null)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => AddLog(message, color));
                return;
            }

            AddLogFast(message, color);
            ScrollToEndDelayed();
        }

        private async void ScrollToEndDelayed()
        {
            // Небольшая задержка перед скроллом
            await Task.Delay(10);
            DebugBox.ScrollToEnd();
        }

        /// <summary>
        /// Очистить лог
        /// </summary>
        public void ClearLog()
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(ClearLog);
                return;
            }

            // Более быстрый способ очистки
            var flowDoc = DebugBox.Document;
            flowDoc.Blocks.Clear();

            // Добавляем пустой параграф для структуры
            flowDoc.Blocks.Add(new Paragraph());
        }

        /// <summary>
        /// Обновить последнюю строку лога
        /// </summary>
        public void UpdateLastLog(string newMessage, Brush? color = null)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => UpdateLastLog(newMessage, color));
                return;
            }

            if (DebugBox.Document.Blocks.LastBlock is Paragraph lastParagraph)
            {
                lastParagraph.Inlines.Clear();
                lastParagraph.Inlines.Add(new Run(newMessage)
                {
                    Foreground = color ?? Brushes.White
                });
            }
        }

        private bool CheckAccess()
        {
            return Dispatcher.CheckAccess();
        }
    }
}