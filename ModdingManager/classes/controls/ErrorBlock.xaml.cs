using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace ModdingManager.classes.controls
{
    /// <summary>
    /// Логика взаимодействия для ErrorBlock.xaml
    /// </summary>
    public enum ErrorType
    {
        Warning,
        Critical
    }

    public partial class ErrorBlock : UserControl
    {
        public static readonly DependencyProperty ErrorTypeProperty =
            DependencyProperty.Register("ErrorType", typeof(ErrorType), typeof(ErrorBlock),
            new PropertyMetadata(ErrorType.Warning, OnErrorTypeChanged));

        public static readonly DependencyProperty SourcePathProperty =
            DependencyProperty.Register("SourcePath", typeof(string), typeof(ErrorBlock));

        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register("ErrorMessage", typeof(string), typeof(ErrorBlock),
            new PropertyMetadata(string.Empty, OnErrorMessageChanged));

        public ErrorBlock()
        {
            InitializeComponent();
            UpdateErrorIcon();
            SizeChanged += OnSizeChanged;
        }

        public ErrorType ErrorType
        {
            get => (ErrorType)GetValue(ErrorTypeProperty);
            set => SetValue(ErrorTypeProperty, value);
        }

        public string SourcePath
        {
            get => (string)GetValue(SourcePathProperty);
            set => SetValue(SourcePathProperty, value);
        }

        public string ErrorMessage
        {
            get => (string)GetValue(ErrorMessageProperty);
            set => SetValue(ErrorMessageProperty, value);
        }

        private static void OnErrorTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ErrorBlock errorBlock)
            {
                errorBlock.UpdateErrorIcon();
            }
        }

        private static void OnErrorMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ErrorBlock errorBlock)
            {
                errorBlock.ErrorText.Text = e.NewValue as string;
            }
        }

        private void UpdateErrorIcon()
        {
            var icon = ErrorType switch
            {
                ErrorType.Warning => new Uri("pack://application:,,,/ModdingManager;component/graphics/controls/warn.png", UriKind.Absolute),
                ErrorType.Critical => new Uri("pack://application:,,,/ModdingManager;component/graphics/controls/err.png", UriKind.Absolute),
                _ => throw new ArgumentOutOfRangeException()
            };
            ErrorIcon.Source = ErrorIcon.Source = new BitmapImage(icon);
        }
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double imageTotalHeight = ErrorIcon.ActualHeight + ErrorIcon.Margin.Top + ErrorIcon.Margin.Bottom;
            double textTotalHeight = ErrorText.ActualHeight + ErrorText.Margin.Top + ErrorText.Margin.Bottom;
            Height = Math.Max(imageTotalHeight, textTotalHeight);
        }

        private void UserControl_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(SourcePath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = SourcePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
