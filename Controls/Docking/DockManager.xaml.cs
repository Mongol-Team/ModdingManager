using Application.Debugging;
using Application.utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using UserControl = System.Windows.Controls.UserControl;

namespace Controls.Docking
{
    public partial class DockManager : UserControl
    {
        private readonly Dictionary<DockSide, DockZone> _zones = new();
        private readonly Dictionary<DockPanelInfo, RoutedEventHandler> _panelClosedHandlers = new();
        private bool _layoutUpdateQueued;

        private readonly Dictionary<DockSide, GridLength> _zoneSizes = new()
        {
            [DockSide.Left] = new GridLength(300, GridUnitType.Pixel),
            [DockSide.Right] = new GridLength(300, GridUnitType.Pixel),
            [DockSide.Top] = new GridLength(200, GridUnitType.Pixel),
            [DockSide.Bottom] = new GridLength(200, GridUnitType.Pixel)
        };

        private readonly Dictionary<DockSide, GridLength> _lastValidSizes = new();

        private const double SplitterThickness = 5;
        private const int ColLeft = 0;
        private const int ColLeftSplit = 1;
        private const int ColCenter = 2;
        private const int ColRightSplit = 3;
        private const int ColRight = 4;
        private const int RowTop = 0;
        private const int RowTopSplit = 1;
        private const int RowCenter = 2;
        private const int RowBottomSplit = 3;
        private const int RowBottom = 4;

        public DockManager()
        {
            InitializeComponent();

            InitializeZones();
            EnsureGridStructure();
            PlaceChildrenOnce();
            AttachSplitterHandlers();

            QueueLayoutUpdate();
        }

        // ──────────────────────────────────────────────
        // Публичный API
        // ──────────────────────────────────────────────

        public void AddPanel(DockPanelInfo panel, DockSide side)
        {
            if (panel == null) return;

            if (_zones.TryGetValue(side, out var zone))
            {
                RemovePanel(panel);
                zone.AddPanel(panel);

                if (!_panelClosedHandlers.ContainsKey(panel))
                {
                    RoutedEventHandler handler = (_, __) => RemovePanel(panel);
                    _panelClosedHandlers[panel] = handler;
                    panel.Closed += handler;
                }

                QueueLayoutUpdate();
            }
        }
        /// <summary>
        /// Обновляет Content у TabItem панели в зоне где она находится.
        /// Нужно вызывать после программного изменения DockPanelInfo.Content.
        /// </summary>
        public void RefreshPanelContent(DockPanelInfo panel)
        {
            if (panel == null) return;

            foreach (var (side, zone) in _zones)
            {
                if (zone.Panels.Contains(panel))
                {
                    zone.RefreshPanelContent(panel);
                    Logger.AddDbgLog(StaticLocalisation.GetString(
                        "Log.DockManager.PanelContentRefreshed", panel.Title));
                    return;
                }
            }
        }
        public void RemovePanel(DockPanelInfo panel)
        {
            if (panel == null) return;

            foreach (var zone in _zones.Values)
            {
                if (zone.Panels.Contains(panel))
                {
                    zone.RemovePanel(panel);
                    break;
                }
            }

            if (_panelClosedHandlers.TryGetValue(panel, out var handler))
            {
                panel.Closed -= handler;
                _panelClosedHandlers.Remove(panel);
            }

            QueueLayoutUpdate();
        }

        /// <summary>
        /// Активирует вкладку соответствующую указанной панели в зоне где она находится.
        /// Используется для программного переключения на GenericViewer / FileExplorer.
        /// </summary>
        public void ActivatePanel(DockPanelInfo panel)
        {
            if (panel == null) return;

            foreach (var zone in _zones.Values)
            {
                if (zone.Panels.Contains(panel))
                {
                    zone.SelectPanel(panel);
                    return;
                }
            }
        }

        public void NavigateToPage(Page page, string? title = null)
        {
            if (page == null) return;

            CenterFrame.Navigate(page);

            if (!string.IsNullOrEmpty(title))
                page.Title = title;
        }

        public void SetContent(UIElement content)
        {
            if (content == null) return;

            CenterFrame.NavigationService?.StopLoading();
            CenterFrame.Content = null;
            CenterFrame.Content = content;
            CenterFrame.UpdateLayout();
        }

        public DockZone? GetZone(DockSide side)
        {
            return _zones.TryGetValue(side, out var zone) ? zone : null;
        }

        public IEnumerable<DockPanelInfo> GetAllPanels()
        {
            return _zones.Values.SelectMany(z => z.Panels);
        }

        // ──────────────────────────────────────────────
        // Остальной существующий код без изменений
        // ──────────────────────────────────────────────

        private void AttachSplitterHandlers()
        {
            LeftSplitter.DragCompleted += (s, e) => SaveZoneSizes();
            RightSplitter.DragCompleted += (s, e) => SaveZoneSizes();
            TopSplitter.DragCompleted += (s, e) => SaveZoneSizes();
            BottomSplitter.DragCompleted += (s, e) => SaveZoneSizes();
        }

        private void SaveZoneSizes()
        {
            if (MainGrid.ColumnDefinitions.Count != 5 || MainGrid.RowDefinitions.Count != 5)
                return;

            if (LeftZone.Visibility == Visibility.Visible)
            {
                var w = MainGrid.ColumnDefinitions[ColLeft].ActualWidth;
                if (w > 0) { _zoneSizes[DockSide.Left] = new GridLength(w); _lastValidSizes[DockSide.Left] = new GridLength(w); }
            }
            if (RightZone.Visibility == Visibility.Visible)
            {
                var w = MainGrid.ColumnDefinitions[ColRight].ActualWidth;
                if (w > 0) { _zoneSizes[DockSide.Right] = new GridLength(w); _lastValidSizes[DockSide.Right] = new GridLength(w); }
            }
            if (TopZone.Visibility == Visibility.Visible)
            {
                var h = MainGrid.RowDefinitions[RowTop].ActualHeight;
                if (h > 0) { _zoneSizes[DockSide.Top] = new GridLength(h); _lastValidSizes[DockSide.Top] = new GridLength(h); }
            }
            if (BottomZone.Visibility == Visibility.Visible)
            {
                var h = MainGrid.RowDefinitions[RowBottom].ActualHeight;
                if (h > 0) { _zoneSizes[DockSide.Bottom] = new GridLength(h); _lastValidSizes[DockSide.Bottom] = new GridLength(h); }
            }
        }

        // ── 1. Событие (в поля класса) ───────────────────────────────────
        /// <summary>
        /// Пробрасывает PanelSelectionChanged из любой DockZone.
        /// Параметры: сторона зоны + выбранная панель.
        /// </summary>
        public event Action<DockSide, DockPanelInfo> PanelSelectionChanged;

        // ── 2. Заменить метод InitializeZones ────────────────────────────
        private void InitializeZones()
        {
            _zones[DockSide.Left] = LeftZone;
            _zones[DockSide.Right] = RightZone;
            _zones[DockSide.Top] = TopZone;
            _zones[DockSide.Bottom] = BottomZone;

            foreach (var (side, zone) in _zones)
            {
                zone.Panels.CollectionChanged += OnZonePanelsChanged;

                // Захватываем side в локальной переменной чтобы замыкание работало правильно
                var capturedSide = side;
                zone.PanelSelectionChanged += panel =>
                    PanelSelectionChanged?.Invoke(capturedSide, panel);
            }

            Unloaded += (_, __) => Cleanup();
        }

        private void Cleanup()
        {
            foreach (var zone in _zones.Values)
                zone.Panels.CollectionChanged -= OnZonePanelsChanged;

            foreach (var kv in _panelClosedHandlers.ToList())
                kv.Key.Closed -= kv.Value;

            _panelClosedHandlers.Clear();
        }

        private void OnZonePanelsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            QueueLayoutUpdate();
        }

        private void QueueLayoutUpdate()
        {
            if (_layoutUpdateQueued) return;

            _layoutUpdateQueued = true;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _layoutUpdateQueued = false;
                UpdateLayoutState();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void EnsureGridStructure()
        {
            if (MainGrid.ColumnDefinitions.Count == 5 && MainGrid.RowDefinitions.Count == 5)
                return;

            MainGrid.ColumnDefinitions.Clear();
            MainGrid.RowDefinitions.Clear();

            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = _zoneSizes[DockSide.Left], MinWidth = 100 });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(SplitterThickness), MaxWidth = SplitterThickness });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 100 });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(SplitterThickness), MaxWidth = SplitterThickness });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = _zoneSizes[DockSide.Right], MinWidth = 100 });

            MainGrid.RowDefinitions.Add(new RowDefinition { Height = _zoneSizes[DockSide.Top], MinHeight = 100 });
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(SplitterThickness), MaxHeight = SplitterThickness });
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 100 });
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(SplitterThickness), MaxHeight = SplitterThickness });
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = _zoneSizes[DockSide.Bottom], MinHeight = 100 });
        }

        private void PlaceChildrenOnce()
        {
            Grid.SetColumn(LeftZone, ColLeft);
            Grid.SetRow(LeftZone, RowCenter);
            Grid.SetColumn(LeftSplitter, ColLeftSplit);
            Grid.SetRow(LeftSplitter, RowCenter);

            Grid.SetColumn(RightZone, ColRight);
            Grid.SetRow(RightZone, RowCenter);
            Grid.SetColumn(RightSplitter, ColRightSplit);
            Grid.SetRow(RightSplitter, RowCenter);

            Grid.SetColumn(TopZone, ColCenter);
            Grid.SetRow(TopZone, RowTop);
            Grid.SetColumn(TopSplitter, ColCenter);
            Grid.SetRow(TopSplitter, RowTopSplit);
            Grid.SetColumnSpan(TopSplitter, 3);

            Grid.SetColumn(BottomZone, ColCenter);
            Grid.SetRow(BottomZone, RowBottom);
            Grid.SetColumn(BottomSplitter, ColCenter);
            Grid.SetRow(BottomSplitter, RowBottomSplit);
            Grid.SetColumnSpan(BottomSplitter, 3);
        }

        private void UpdateLayoutState()
        {
            bool hasTop = TopZone.HasPanels;
            bool hasBottom = BottomZone.HasPanels;
            bool hasLeft = LeftZone.HasPanels;
            bool hasRight = RightZone.HasPanels;

            int contentCol = hasLeft ? ColCenter : ColLeft;
            int contentColSpan = 1 + (hasLeft ? 0 : 2) + (hasRight ? 0 : 2);
            int contentRow = hasTop ? RowCenter : RowTop;
            int contentRowSpan = 1 + (hasTop ? 0 : 2) + (hasBottom ? 0 : 2);

            Grid.SetColumn(CenterFrame, contentCol);
            Grid.SetColumnSpan(CenterFrame, contentColSpan);
            Grid.SetRow(CenterFrame, contentRow);
            Grid.SetRowSpan(CenterFrame, contentRowSpan);

            Grid.SetColumn(TopZone, contentCol); Grid.SetColumnSpan(TopZone, contentColSpan);
            Grid.SetColumn(TopSplitter, contentCol); Grid.SetColumnSpan(TopSplitter, contentColSpan);
            Grid.SetColumn(BottomZone, contentCol); Grid.SetColumnSpan(BottomZone, contentColSpan);
            Grid.SetColumn(BottomSplitter, contentCol); Grid.SetColumnSpan(BottomSplitter, contentColSpan);

            Grid.SetRow(LeftZone, contentRow); Grid.SetRowSpan(LeftZone, contentRowSpan);
            Grid.SetRow(LeftSplitter, contentRow); Grid.SetRowSpan(LeftSplitter, contentRowSpan);
            Grid.SetRow(RightZone, contentRow); Grid.SetRowSpan(RightZone, contentRowSpan);
            Grid.SetRow(RightSplitter, contentRow); Grid.SetRowSpan(RightSplitter, contentRowSpan);

            TopZone.Visibility = hasTop ? Visibility.Visible : Visibility.Collapsed;
            TopSplitter.Visibility = hasTop ? Visibility.Visible : Visibility.Collapsed;
            BottomZone.Visibility = hasBottom ? Visibility.Visible : Visibility.Collapsed;
            BottomSplitter.Visibility = hasBottom ? Visibility.Visible : Visibility.Collapsed;
            LeftZone.Visibility = hasLeft ? Visibility.Visible : Visibility.Collapsed;
            LeftSplitter.Visibility = hasLeft ? Visibility.Visible : Visibility.Collapsed;
            RightZone.Visibility = hasRight ? Visibility.Visible : Visibility.Collapsed;
            RightSplitter.Visibility = hasRight ? Visibility.Visible : Visibility.Collapsed;

            if (MainGrid.ColumnDefinitions.Count == 5 && MainGrid.RowDefinitions.Count == 5)
            {
                SetZoneSize(hasLeft, DockSide.Left, ColLeft, isColumn: true);
                SetZoneSize(hasRight, DockSide.Right, ColRight, isColumn: true);
                SetZoneSize(hasTop, DockSide.Top, RowTop, isColumn: false);
                SetZoneSize(hasBottom, DockSide.Bottom, RowBottom, isColumn: false);

                MainGrid.ColumnDefinitions[ColLeftSplit].Width = hasLeft ? new GridLength(SplitterThickness) : new GridLength(0);
                MainGrid.ColumnDefinitions[ColRightSplit].Width = hasRight ? new GridLength(SplitterThickness) : new GridLength(0);
                MainGrid.RowDefinitions[RowTopSplit].Height = hasTop ? new GridLength(SplitterThickness) : new GridLength(0);
                MainGrid.RowDefinitions[RowBottomSplit].Height = hasBottom ? new GridLength(SplitterThickness) : new GridLength(0);
            }
        }

        private void SetZoneSize(bool hasZone, DockSide side, int index, bool isColumn)
        {
            if (hasZone)
            {
                var size = _lastValidSizes.TryGetValue(side, out var valid) ? valid : _zoneSizes[side];
                if (isColumn)
                    MainGrid.ColumnDefinitions[index].Width = size;
                else
                    MainGrid.RowDefinitions[index].Height = size;
            }
            else
            {
                if (isColumn)
                    MainGrid.ColumnDefinitions[index].Width = new GridLength(0);
                else
                    MainGrid.RowDefinitions[index].Height = new GridLength(0);
            }
        }


    }
}