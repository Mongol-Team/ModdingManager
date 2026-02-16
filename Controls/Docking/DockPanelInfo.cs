using System.Windows;

namespace Controls.Docking
{
    public class DockPanelInfo
    {
        public string Title { get; set; }
        public UIElement Content { get; set; }
        public bool CanClose { get; set; } = true;
        public bool CanPin { get; set; } = true;
        public bool IsPinned { get; set; } = true;

        public event RoutedEventHandler Closed;
        public event RoutedEventHandler PinnedChanged;
        public event System.Windows.Input.MouseButtonEventHandler DragStarted;

        public void RaiseClosed()
        {
            Closed?.Invoke(this, new RoutedEventArgs());
        }

        public void RaisePinnedChanged()
        {
            PinnedChanged?.Invoke(this, new RoutedEventArgs());
        }

        public void RaiseDragStarted(System.Windows.Input.MouseButtonEventArgs e)
        {
            DragStarted?.Invoke(this, e);
        }
    }
}

