using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;
using Orientation = System.Windows.Controls.Orientation;
using UserControl = System.Windows.Controls.UserControl;

namespace ViewControls.Docking
{
    public enum DockSide
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public partial class DockZone : UserControl
    {
        public static readonly DependencyProperty DockSideProperty =
            DependencyProperty.Register(nameof(DockSide), typeof(DockSide), typeof(DockZone),
                new PropertyMetadata(DockSide.Left));

        public static readonly DependencyProperty IsCollapsedProperty =
            DependencyProperty.Register(nameof(IsCollapsed), typeof(bool), typeof(DockZone),
                new PropertyMetadata(false, OnIsCollapsedChanged));

        public static readonly DependencyProperty HasPanelsProperty =
            DependencyProperty.Register(nameof(HasPanels), typeof(bool), typeof(DockZone),
                new PropertyMetadata(false));

        private ObservableCollection<DockPanelInfo> _panels = new();

        public DockSide DockSide
        {
            get => (DockSide)GetValue(DockSideProperty);
            set => SetValue(DockSideProperty, value);
        }

        public bool IsCollapsed
        {
            get => (bool)GetValue(IsCollapsedProperty);
            set => SetValue(IsCollapsedProperty, value);
        }

        public bool HasPanels
        {
            get => (bool)GetValue(HasPanelsProperty);
            private set => SetValue(HasPanelsProperty, value);
        }

        public ObservableCollection<DockPanelInfo> Panels => _panels;

        public DockZone()
        {
            InitializeComponent();
            _panels.CollectionChanged += Panels_CollectionChanged;
        }

        private static void OnIsCollapsedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DockZone zone)
            {
                zone.UpdateVisibility();
            }
        }

        private void Panels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdatePanels();
            UpdateVisibility();
        }

        public void AddPanel(DockPanelInfo panel)
        {
            if (panel == null) return;

            if (!_panels.Contains(panel))
            {
                var existingPanel = _panels.FirstOrDefault(p => p.Title == panel.Title);
                if (existingPanel == null)
                {
                    _panels.Add(panel);
                }
            }
        }

        public void RemovePanel(DockPanelInfo panel)
        {
            _panels.Remove(panel);
        }

        private void UpdatePanels()
        {
            PanelsTabControl.Items.Clear();

            foreach (var panel in _panels)
            {
                var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
                
                var titleBlock = new TextBlock
                {
                    Text = panel.Title,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 8, 0)
                };
                headerPanel.Children.Add(titleBlock);

                if (panel.CanPin)
                {
                    var pinButton = new Button
                    {
                        Content = panel.IsPinned ? "📌" : "📍",
                        Width = 20,
                        Height = 20,
                        Padding = new Thickness(0, 0, 0, 0),
                        Margin = new Thickness(2, 0, 2, 0),
                        Style = (Style)System.Windows.Application.Current.Resources["WindowControlButton"]
                    };
                    pinButton.Click += (s, e) =>
                    {
                        panel.IsPinned = !panel.IsPinned;
                        pinButton.Content = panel.IsPinned ? "📌" : "📍";
                        panel.RaisePinnedChanged();
                    };
                    headerPanel.Children.Add(pinButton);
                }

                if (panel.CanClose)
                {
                    var closeButton = new Button
                    {
                        Content = "✕",
                        Width = 20,
                        Height = 20,
                        Padding = new Thickness(0, 0, 0, 0),
                        Margin = new Thickness(2, 0, 2, 0),
                        Style = (Style)System.Windows.Application.Current.Resources["WindowControlButton"]
                    };
                    closeButton.Click += (s, e) => panel.RaiseClosed();
                    headerPanel.Children.Add(closeButton);
                }

                var tabItem = new TabItem
                {
                    Header = headerPanel,
                    Content = panel.Content
                };
                PanelsTabControl.Items.Add(tabItem);
            }

            HasPanels = _panels.Count > 0;
        }

        private void UpdateVisibility()
        {
            Visibility = (HasPanels && !IsCollapsed)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}

