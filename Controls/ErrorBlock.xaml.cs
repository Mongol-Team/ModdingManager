using Application.Extentions;
using Models.Enums;
using Models.Interfaces;           // здесь находится IError
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace Controls
{
    /// <summary>
    /// Контрол для отображения одной ошибки, совместимый с интерфейсом Models.Interfaces.IError
    /// </summary>
    public partial class ErrorBlock : UserControl
    {
        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register(
                nameof(Error),
                typeof(IError),
                typeof(ErrorBlock),
                new PropertyMetadata(null, OnErrorChanged));

        public IError? Error
        {
            get => (IError?)GetValue(ErrorProperty);
            set => SetValue(ErrorProperty, value);
        }

        public ErrorBlock()
        {
            InitializeComponent();
            SizeChanged += OnSizeChanged;
        }

        private static void OnErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ErrorBlock errorBlock)
                return;

            if (e.NewValue is not IError error)
            {
                errorBlock.Clear();
                return;
            }

            // Обновляем содержимое
            errorBlock.ErrorText.Text = error.Message ?? string.Empty;

            // Определяем тип иконки
            var icon = error.Type switch
            {
                ErrorType.Warn => Properties.Resources.warn,
                ErrorType.Critical => Properties.Resources.err,
                ErrorType.Fatal => Properties.Resources.err, // если есть отдельная иконка
                ErrorType.Info => Properties.Resources.warn,
                _ => Properties.Resources.err,
            };

            errorBlock.ErrorIcon.Source = icon.ToBitmapSource();

            // Можно также использовать IsFatal для визуального выделения, если хотите
            // Например: более яркий фон или красная рамка
            if (error.Type == ErrorType.Fatal)
            {
                errorBlock.Background = new SolidColorBrush(Color.FromArgb(40, 255, 0, 0));
            }
            else
            {
                errorBlock.Background = Brushes.Transparent;
            }

            // Подсказка/тултип с дополнительной информацией
            errorBlock.ToolTip = $"Type: {error.Type}\n" +
                                $"File: {error.Path}\n" +
                                $"Line: {error.Line}\n" +
                                (error.IsGameError ? "Game-related error" : "");
        }

        private void Clear()
        {
            ErrorText.Text = string.Empty;
            ErrorIcon.Source = null;
            Background = Brushes.Transparent;
            ToolTip = null;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double imageTotalHeight = ErrorIcon.ActualHeight + ErrorIcon.Margin.Top + ErrorIcon.Margin.Bottom;
            double textTotalHeight = ErrorText.ActualHeight + ErrorText.Margin.Top + ErrorText.Margin.Bottom;
            Height = Math.Max(imageTotalHeight, textTotalHeight) + 4; // небольшой запас
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Error?.Path is { Length: > 0 } path)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось открыть файл:\n{path}\n\n{ex.Message}",
                        "Ошибка открытия файла",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}