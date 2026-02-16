using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace Controls
{
    public partial class MessageDialog : Window
    {
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public MessageBoxButton ButtonType { get; set; } = MessageBoxButton.OK;
        public MessageBoxImage ImageType { get; set; } = MessageBoxImage.None;

        public MessageDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        public MessageDialog(string message, string title = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None)
            : this()
        {
            Message = message;
            Title = title;
            ButtonType = button;
            ImageType = image;
            SetupDialog();
        }

        private void SetupDialog()
        {
            // Настройка кнопок
            switch (ButtonType)
            {
                case MessageBoxButton.OK:
                    OkButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Collapsed;
                    YesButton.Visibility = Visibility.Collapsed;
                    NoButton.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.OKCancel:
                    OkButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;
                    YesButton.Visibility = Visibility.Collapsed;
                    NoButton.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.YesNo:
                    OkButton.Visibility = Visibility.Collapsed;
                    CancelButton.Visibility = Visibility.Collapsed;
                    YesButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNoCancel:
                    OkButton.Visibility = Visibility.Collapsed;
                    CancelButton.Visibility = Visibility.Visible;
                    YesButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    break;
            }

            // Настройка иконки
            SetupIcon();
        }

        private void SetupIcon()
        {
            Geometry iconGeometry = null;
            Brush iconBrush = null;

            switch (ImageType)
            {
                case MessageBoxImage.Error:
                    iconGeometry = Geometry.Parse("M 12,2 C 6.48,2 2,6.48 2,12 C 2,17.52 6.48,22 12,22 C 17.52,22 22,17.52 22,12 C 22,6.48 17.52,2 12,2 Z M 13,17 L 11,17 L 11,15 L 13,15 L 13,17 Z M 13,13 L 11,13 L 11,7 L 13,7 L 13,13 Z");
                    iconBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f48771"));
                    break;
                case MessageBoxImage.Warning:
                    iconGeometry = Geometry.Parse("M 12,2 L 2,22 L 22,22 Z M 12,6 L 12,14 M 12,18 L 12,18");
                    iconBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#cca700"));
                    break;
                case MessageBoxImage.Information:
                    iconGeometry = Geometry.Parse("M 12,2 C 6.48,2 2,6.48 2,12 C 2,17.52 6.48,22 12,22 C 17.52,22 22,17.52 22,12 C 22,6.48 17.52,2 12,2 Z M 13,17 L 11,17 L 11,11 L 13,11 L 13,17 Z M 12,8 C 11.45,8 11,7.55 11,7 C 11,6.45 11.45,6 12,6 C 12.55,6 13,6.45 13,7 C 13,7.55 12.55,8 12,8 Z");
                    iconBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007acc"));
                    break;
                case MessageBoxImage.Question:
                    iconGeometry = Geometry.Parse("M 12,2 C 6.48,2 2,6.48 2,12 C 2,17.52 6.48,22 12,22 C 17.52,22 22,17.52 22,12 C 22,6.48 17.52,2 12,2 Z M 13,19 L 11,19 L 11,17 L 13,17 L 13,19 Z M 15.07,11.25 L 14.17,12.17 C 13.45,12.9 13,13.5 13,15 L 11,15 L 11,14.5 C 11,13.67 11.45,12.9 12.17,12.17 L 13.1,11.25 C 13.45,10.9 13.67,10.45 13.67,10 C 13.67,9.03 12.97,8.33 12,8.33 C 11.03,8.33 10.33,9.03 10.33,10 L 8.33,10 C 8.33,7.76 10.09,6 12.33,6 C 14.57,6 16.33,7.76 16.33,10 C 16.33,10.88 16.05,11.7 15.07,11.25 Z");
                    iconBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007acc"));
                    break;
                default:
                    IconPath.Visibility = Visibility.Collapsed;
                    return;
            }

            if (iconGeometry != null && IconPath != null)
            {
                IconPath.Data = iconGeometry;
                IconPath.Fill = iconBrush;
                IconPath.Visibility = Visibility.Visible;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            DialogResult = false;
            Close();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            DialogResult = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            DialogResult = false;
            Close();
        }

        public static MessageBoxResult Show(string message, string title = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None)
        {
            var dialog = new MessageDialog(message, title, button, image)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            dialog.ShowDialog();
            return dialog.Result;
        }
    }
}

