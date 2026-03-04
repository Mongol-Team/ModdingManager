using Application.Debugging;
using Application.utils;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;
using Orientation = System.Windows.Controls.Orientation;
using UserControl = System.Windows.Controls.UserControl;

namespace Controls.Docking
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
                zone.UpdateVisibility();
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
                var existingPanel = _panels.FirstOrDefault(p => ReferenceEquals(p, panel));
                if (existingPanel == null)
                    _panels.Add(panel);
            }
        }
        /// <summary>
        /// Обновляет Content у TabItem связанного с данной панелью.
        /// Нужно вызывать когда panel.Content меняется уже после построения TabItem-ов
        /// (DockPanelInfo.Content — обычное свойство без уведомления DockZone).
        /// </summary>
        public void RefreshPanelContent(DockPanelInfo panel)
        {
            if (panel == null) return;

            foreach (TabItem item in PanelsTabControl.Items)
            {
                if (ReferenceEquals(item.Tag, panel))
                {
                    item.Content = panel.Content;
                    Logger.AddDbgLog(StaticLocalisation.GetString(
                        "Log.DockZone.PanelContentRefreshed", panel.Title));
                    return;
                }
            }

            Logger.AddDbgLog(StaticLocalisation.GetString(
                "Log.DockZone.RefreshPanelContentNotFound", panel.Title));
        }
        public void RemovePanel(DockPanelInfo panel)
        {
            _panels.Remove(panel);
        }

        /// <summary>
        /// Активирует (выбирает) вкладку соответствующую указанной панели.
        /// </summary>
        public void SelectPanel(DockPanelInfo panel)
        {
            if (panel == null) return;

            foreach (TabItem item in PanelsTabControl.Items)
            {
                if (ReferenceEquals(item.Tag, panel))   
                {
                    PanelsTabControl.SelectedItem = item;
                    return;
                }
            }
        }
        private DockPanelInfo _selectedPanel;

        // ── 2. Событие (после существующих DP) ───────────────────────────
        /// <summary>
        /// Срабатывает когда пользователь или код переключает активную вкладку.
        /// </summary>
        public event Action<DockPanelInfo> PanelSelectionChanged;



        // ── 4. Обработчик SelectionChanged ───────────────────────────────
        // Добавить в UpdatePanels() ДО строки PanelsTabControl.Items.Clear():
        //   PanelsTabControl.SelectionChanged -= OnTabSelectionChanged;
        // Добавить в UpdatePanels() В КОНЦЕ (после HasPanels = ...):
        //   PanelsTabControl.SelectionChanged += OnTabSelectionChanged;

        private void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PanelsTabControl.SelectedItem is not TabItem selected) return;

            var panel = selected.Tag as DockPanelInfo;
            if (panel == null || ReferenceEquals(panel, _selectedPanel)) return;

            _selectedPanel = panel;
            PanelSelectionChanged?.Invoke(panel);
        }

        // ── 5. Итоговый UpdatePanels — заменить существующий ─────────────

        private void UpdatePanels()
        {
            PanelsTabControl.SelectionChanged -= OnTabSelectionChanged;
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
                    Content = panel.Content,
                    Tag = panel
                };
                PanelsTabControl.Items.Add(tabItem);
            }
            PanelsTabControl.SelectionChanged += OnTabSelectionChanged;
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