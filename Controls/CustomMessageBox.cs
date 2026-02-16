using System.Windows;

namespace Controls
{
    public static class CustomMessageBox
    {
        public static MessageBoxResult Show(string message, string title = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None)
        {
            return MessageDialog.Show(message, title, button, image);
        }

        public static MessageBoxResult Show(string message, string title, MessageBoxButton button)
        {
            return MessageDialog.Show(message, title, button, MessageBoxImage.None);
        }

        public static MessageBoxResult Show(string message, string title)
        {
            return MessageDialog.Show(message, title, MessageBoxButton.OK, MessageBoxImage.None);
        }

        public static MessageBoxResult Show(string message)
        {
            return MessageDialog.Show(message, "", MessageBoxButton.OK, MessageBoxImage.None);
        }
    }
}

