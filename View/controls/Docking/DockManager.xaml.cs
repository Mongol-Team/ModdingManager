using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using UserControl = System.Windows.Controls.UserControl;

namespace ViewControls.Docking
{
    public partial class DockManager : UserControl
    {
        private readonly Dictionary<DockSide, DockZone> _zones = new();

        // ����� ����� ���� ������������ �� Closed
        private readonly Dictionary<DockPanelInfo, RoutedEventHandler> _panelClosedHandlers = new();

        // Coalesce layout updates
        private bool _layoutUpdateQueued;

        // ��������� ������� (���� ����� �������� ���������� �������� ������ ����� �������������� � ���� ��)
        private GridLength _leftWidth = new(300, GridUnitType.Pixel);
        private GridLength _rightWidth = new(300, GridUnitType.Pixel);
        private GridLength _topHeight = new(200, GridUnitType.Pixel);
        private GridLength _bottomHeight = new(200, GridUnitType.Pixel);

        private const double SplitterThickness = 5;

        // ������� ������������� ����� 5x5
        // Cols: 0 Left | 1 VSplit | 2 Center | 3 VSplit | 4 Right
        // Rows: 0 Top  | 1 HSplit | 2 Center | 3 HSplit | 4 Bottom
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

        private void AttachSplitterHandlers()
        {
            LeftSplitter.DragCompleted += (s, e) => SaveZoneSizes();
            RightSplitter.DragCompleted += (s, e) => SaveZoneSizes();
            TopSplitter.DragCompleted += (s, e) => SaveZoneSizes();
            BottomSplitter.DragCompleted += (s, e) => SaveZoneSizes();
        }

        private void SaveZoneSizes()
        {
            if (MainGrid.ColumnDefinitions.Count == 5 && MainGrid.RowDefinitions.Count == 5)
            {
                if (LeftZone.Visibility == Visibility.Visible)
                {
                    var width = MainGrid.ColumnDefinitions[ColLeft].ActualWidth;
                    if (width > 0)
                        _leftWidth = new GridLength(width);
                }

                if (RightZone.Visibility == Visibility.Visible)
                {
                    var width = MainGrid.ColumnDefinitions[ColRight].ActualWidth;
                    if (width > 0)
                        _rightWidth = new GridLength(width);
                }

                if (TopZone.Visibility == Visibility.Visible)
                {
                    var height = MainGrid.RowDefinitions[RowTop].ActualHeight;
                    if (height > 0)
                        _topHeight = new GridLength(height);
                }

                if (BottomZone.Visibility == Visibility.Visible)
                {
                    var height = MainGrid.RowDefinitions[RowBottom].ActualHeight;
                    if (height > 0)
                        _bottomHeight = new GridLength(height);
                }
            }
        }

        private void InitializeZones()
        {
            _zones[DockSide.Left] = LeftZone;
            _zones[DockSide.Right] = RightZone;
            _zones[DockSide.Top] = TopZone;
            _zones[DockSide.Bottom] = BottomZone;

            foreach (var zone in _zones.Values)
            {
                zone.Panels.CollectionChanged += OnZonePanelsChanged;
            }

            Unloaded += (_, __) => Cleanup();
        }

        private void Cleanup()
        {
            foreach (var zone in _zones.Values)
            {
                zone.Panels.CollectionChanged -= OnZonePanelsChanged;
            }

            // ���������� panel.Closed (���� DockManager �����������)
            foreach (var kv in _panelClosedHandlers.ToList())
            {
                kv.Key.Closed -= kv.Value;
            }
            _panelClosedHandlers.Clear();
        }

        private void OnZonePanelsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            QueueLayoutUpdate();
        }

        private void QueueLayoutUpdate()
        {
            if (_layoutUpdateQueued)
                return;

            _layoutUpdateQueued = true;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                _layoutUpdateQueued = false;
                UpdateLayoutState();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        /// <summary>
        /// ������ ���������� 5x5 ����� ���� ���. ������ ������ ������ Width/Height � Visibility.
        /// </summary>
        private void EnsureGridStructure()
        {
            if (MainGrid.ColumnDefinitions.Count == 5 && MainGrid.RowDefinitions.Count == 5)
                return;

            MainGrid.ColumnDefinitions.Clear();
            MainGrid.RowDefinitions.Clear();

            // Columns
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = _leftWidth, MinWidth = 100 }); // Left
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(SplitterThickness) });           // Left splitter
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });       // Center
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(SplitterThickness) });           // Right splitter
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = _rightWidth, MinWidth = 100 }); // Right

            // Rows
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = _topHeight, MinHeight = 100 });     // Top
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(SplitterThickness) });               // Top splitter
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });            // Center
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(SplitterThickness) });               // Bottom splitter
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = _bottomHeight, MinHeight = 100 }); // Bottom
        }

        /// <summary>
        /// ���������� Grid.Row/Column ��� ����� ���� ���. ������ �� ������� spans/�������.
        /// </summary>
        private void PlaceChildrenOnce()
        {
            // Zones
            Grid.SetColumn(LeftZone, ColLeft);
            Grid.SetRow(LeftZone, RowCenter);

            Grid.SetColumn(RightZone, ColRight);
            Grid.SetRow(RightZone, RowCenter);

            Grid.SetColumn(TopZone, ColCenter);
            Grid.SetRow(TopZone, RowTop);

            Grid.SetColumn(BottomZone, ColCenter);
            Grid.SetRow(BottomZone, RowBottom);

            // Center
            Grid.SetColumn(CenterFrame, ColCenter);
            Grid.SetRow(CenterFrame, RowCenter);

            // Splitters
            Grid.SetColumn(LeftSplitter, ColLeftSplit);
            Grid.SetRow(LeftSplitter, RowCenter);

            Grid.SetColumn(RightSplitter, ColRightSplit);
            Grid.SetRow(RightSplitter, RowCenter);

            Grid.SetColumn(TopSplitter, ColCenter);
            Grid.SetRow(TopSplitter, RowTopSplit);

            Grid.SetColumn(BottomSplitter, ColCenter);
            Grid.SetRow(BottomSplitter, RowBottomSplit);
        }

        /// <summary>
        /// ������ ����� ������: ����������/�������� ���� � ��������� + ��������� ������/������� � 0.
        /// </summary>
        private void UpdateLayoutState()
        {
            bool hasTop = TopZone.HasPanels;
            bool hasBottom = BottomZone.HasPanels;
            bool hasLeft = LeftZone.HasPanels;
            bool hasRight = RightZone.HasPanels;

            // ����� �������/���� ������� �������� ��� ����������� �������
            int contentCol = hasLeft ? ColCenter : ColLeft;              // ���� ����� ����� � �������� � 0
            int contentColSpan = 1
                + (hasLeft ? 0 : 2)   // ����������� Left + LeftSplitter
                + (hasRight ? 0 : 2); // ����������� RightSplitter + Right

            int contentRow = hasTop ? RowCenter : RowTop;
            int contentRowSpan = 1
                + (hasTop ? 0 : 2)    // ����������� Top + TopSplitter
                + (hasBottom ? 0 : 2);// ����������� BottomSplitter + Bottom

            // CenterFrame �������� �� ��������� �����
            Grid.SetColumn(CenterFrame, contentCol);
            Grid.SetColumnSpan(CenterFrame, contentColSpan);
            Grid.SetRow(CenterFrame, contentRow);
            Grid.SetRowSpan(CenterFrame, contentRowSpan);

            // TopZone/BottomZone ���� ����� ����������� �� ������, ���� �����/������ �����
            Grid.SetColumn(TopZone, contentCol);
            Grid.SetColumnSpan(TopZone, contentColSpan);

            Grid.SetColumn(TopSplitter, contentCol);
            Grid.SetColumnSpan(TopSplitter, contentColSpan);

            Grid.SetColumn(BottomZone, contentCol);
            Grid.SetColumnSpan(BottomZone, contentColSpan);

            Grid.SetColumn(BottomSplitter, contentCol);
            Grid.SetColumnSpan(BottomSplitter, contentColSpan);

            // Left/Right ���� ����� ����������� �� ������, ���� ������/����� �����
            Grid.SetRow(LeftZone, contentRow);
            Grid.SetRowSpan(LeftZone, contentRowSpan);

            Grid.SetRow(LeftSplitter, contentRow);
            Grid.SetRowSpan(LeftSplitter, contentRowSpan);

            Grid.SetRow(RightZone, contentRow);
            Grid.SetRowSpan(RightZone, contentRowSpan);

            Grid.SetRow(RightSplitter, contentRow);
            Grid.SetRowSpan(RightSplitter, contentRowSpan);



            // Visibilities
            TopZone.Visibility = hasTop ? Visibility.Visible : Visibility.Collapsed;
            TopSplitter.Visibility = hasTop ? Visibility.Visible : Visibility.Collapsed;

            BottomZone.Visibility = hasBottom ? Visibility.Visible : Visibility.Collapsed;
            BottomSplitter.Visibility = hasBottom ? Visibility.Visible : Visibility.Collapsed;

            LeftZone.Visibility = hasLeft ? Visibility.Visible : Visibility.Collapsed;
            LeftSplitter.Visibility = hasLeft ? Visibility.Visible : Visibility.Collapsed;

            RightZone.Visibility = hasRight ? Visibility.Visible : Visibility.Collapsed;
            RightSplitter.Visibility = hasRight ? Visibility.Visible : Visibility.Collapsed;

            // Width/Height control (0 when �����������)
            if (MainGrid.ColumnDefinitions.Count == 5 && MainGrid.RowDefinitions.Count == 5)
            {
                if (hasLeft)
                {
                    var currentWidth = MainGrid.ColumnDefinitions[ColLeft].ActualWidth;
                    if (currentWidth > 0)
                        _leftWidth = new GridLength(currentWidth);
                    MainGrid.ColumnDefinitions[ColLeft].Width = _leftWidth;
                }
                else
                {
                    MainGrid.ColumnDefinitions[ColLeft].Width = new GridLength(0);
                }
                MainGrid.ColumnDefinitions[ColLeftSplit].Width = hasLeft ? new GridLength(SplitterThickness) : new GridLength(0);

                if (hasRight)
                {
                    var currentWidth = MainGrid.ColumnDefinitions[ColRight].ActualWidth;
                    if (currentWidth > 0)
                        _rightWidth = new GridLength(currentWidth);
                    MainGrid.ColumnDefinitions[ColRight].Width = _rightWidth;
                }
                else
                {
                    MainGrid.ColumnDefinitions[ColRight].Width = new GridLength(0);
                }
                MainGrid.ColumnDefinitions[ColRightSplit].Width = hasRight ? new GridLength(SplitterThickness) : new GridLength(0);

                if (hasTop)
                {
                    var currentHeight = MainGrid.RowDefinitions[RowTop].ActualHeight;
                    if (currentHeight > 0)
                        _topHeight = new GridLength(currentHeight);
                    MainGrid.RowDefinitions[RowTop].Height = _topHeight;
                }
                else
                {
                    MainGrid.RowDefinitions[RowTop].Height = new GridLength(0);
                }
                MainGrid.RowDefinitions[RowTopSplit].Height = hasTop ? new GridLength(SplitterThickness) : new GridLength(0);

                if (hasBottom)
                {
                    var currentHeight = MainGrid.RowDefinitions[RowBottom].ActualHeight;
                    if (currentHeight > 0)
                        _bottomHeight = new GridLength(currentHeight);
                    MainGrid.RowDefinitions[RowBottom].Height = _bottomHeight;
                }
                else
                {
                    MainGrid.RowDefinitions[RowBottom].Height = new GridLength(0);
                }
                MainGrid.RowDefinitions[RowBottomSplit].Height = hasBottom ? new GridLength(SplitterThickness) : new GridLength(0);
            }
        }

        public void AddPanel(DockPanelInfo panel, DockSide side)
        {
            if (panel == null) return;

            if (_zones.TryGetValue(side, out var zone))
            {
                // ���� ������ ��� ���-�� ���� � ������� ������ (������ �� ������)
                RemovePanel(panel);

                zone.AddPanel(panel);

                // �������� �� Closed � ������������ �������
                if (!_panelClosedHandlers.ContainsKey(panel))
                {
                    RoutedEventHandler handler = (_, __) => RemovePanel(panel);
                    _panelClosedHandlers[panel] = handler;
                    panel.Closed += handler;
                }

                QueueLayoutUpdate();
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
    }
}
