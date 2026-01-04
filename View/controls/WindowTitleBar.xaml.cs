using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace ViewControls
{
    public partial class WindowTitleBar : System.Windows.Controls.UserControl
    {
        public WindowTitleBar()
        {
            InitializeComponent();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    if (window.WindowState == WindowState.Maximized)
                    {
                        window.WindowState = WindowState.Normal;
                    }
                    else
                    {
                        window.WindowState = WindowState.Maximized;
                    }
                }
            }
            else if (e.ChangedButton == MouseButton.Left)
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.DragMove();
                }
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.WindowState = WindowState.Maximized;
                MaximizeButton.Visibility = Visibility.Collapsed;
                RestoreButton.Visibility = Visibility.Visible;
            }
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.WindowState = WindowState.Normal;
                MaximizeButton.Visibility = Visibility.Visible;
                RestoreButton.Visibility = Visibility.Collapsed;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.Close();
            }
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.StateChanged += Window_StateChanged;
                UpdateButtonVisibility(window.WindowState);
            }
        }

        private void Window_StateChanged(object sender, System.EventArgs e)
        {
            var window = sender as Window;
            if (window != null)
            {
                UpdateButtonVisibility(window.WindowState);
            }
        }

        private void UpdateButtonVisibility(WindowState state)
        {
            if (state == WindowState.Maximized)
            {
                MaximizeButton.Visibility = Visibility.Collapsed;
                RestoreButton.Visibility = Visibility.Visible;
            }
            else
            {
                MaximizeButton.Visibility = Visibility.Visible;
                RestoreButton.Visibility = Visibility.Collapsed;
            }
        }
    }
}

