using View.Utils;
using System.Windows;
using Application.utils;

namespace View
{
    public partial class PlaceholderWindow : BaseWindow
    {
        public PlaceholderWindow()
        {
            InitializeComponent();
        }

        public PlaceholderWindow(string message) : this()
        {
            if (!string.IsNullOrEmpty(message))
            {
                MessageTextBlock.Text = message;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static void ShowPlaceholder(string message = null, Window owner = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                message = StaticLocalisation.GetString("Message.PageNotReady");
            }
            var window = new PlaceholderWindow(message);
            if (owner != null)
            {
                window.Owner = owner;
            }
            window.ShowDialog();
        }
    }
}

