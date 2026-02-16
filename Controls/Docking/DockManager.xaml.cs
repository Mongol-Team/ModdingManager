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

        // Храним размеры зон при их закрытии
        private readonly Dictionary<DockSide, GridLength> _zoneSizes = new()
        {
            [DockSide.Left] = new GridLength(300, GridUnitType.Pixel),
            [DockSide.Right] = new GridLength(300, GridUnitType.Pixel),
            [DockSide.Top] = new GridLength(200, GridUnitType.Pixel),
            [DockSide.Bottom] = new GridLength(200, GridUnitType.Pixel)
        };

        // Храним размеры зон до их закрытия
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
                // Сохраняем фактические размеры только если зона видима
                if (LeftZone.Visibility == Visibility.Visible)
                {
                    var width = MainGrid.ColumnDefinitions[ColLeft].ActualWidth;
                    if (width > 0)
                    {
                        _zoneSizes[DockSide.Left] = new GridLength(width);
                        _lastValidSizes[DockSide.Left] = new GridLength(width);
                    }
                }

                if (RightZone.Visibility == Visibility.Visible)
                {
                    var width = MainGrid.ColumnDefinitions[ColRight].ActualWidth;
                    if (width > 0)
                    {
                        _zoneSizes[DockSide.Right] = new GridLength(width);
                        _lastValidSizes[DockSide.Right] = new GridLength(width);
                    }
                }

                if (TopZone.Visibility == Visibility.Visible)
                {
                    var height = MainGrid.RowDefinitions[RowTop].ActualHeight;
                    if (height > 0)
                    {
                        _zoneSizes[DockSide.Top] = new GridLength(height);
                        _lastValidSizes[DockSide.Top] = new GridLength(height);
                    }
                }

                if (BottomZone.Visibility == Visibility.Visible)
                {
                    var height = MainGrid.RowDefinitions[RowBottom].ActualHeight;
                    if (height > 0)
                    {
                        _zoneSizes[DockSide.Bottom] = new GridLength(height);
                        _lastValidSizes[DockSide.Bottom] = new GridLength(height);
                    }
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

        private void EnsureGridStructure()
        {
            if (MainGrid.ColumnDefinitions.Count == 5 && MainGrid.RowDefinitions.Count == 5)
                return;

            MainGrid.ColumnDefinitions.Clear();
            MainGrid.RowDefinitions.Clear();

            // Columns
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = _zoneSizes[DockSide.Left],
                MinWidth = 100,
                MaxWidth = double.PositiveInfinity // Убираем ограничение на максимальную ширину
            });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(SplitterThickness),
                MaxWidth = SplitterThickness
            });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star),
                MinWidth = 100
            });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(SplitterThickness),
                MaxWidth = SplitterThickness
            });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = _zoneSizes[DockSide.Right],
                MinWidth = 100,
                MaxWidth = double.PositiveInfinity
            });

            // Rows
            MainGrid.RowDefinitions.Add(new RowDefinition
            {
                Height = _zoneSizes[DockSide.Top],
                MinHeight = 100,
                MaxHeight = double.PositiveInfinity
            });
            MainGrid.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(SplitterThickness),
                MaxHeight = SplitterThickness
            });
            MainGrid.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(1, GridUnitType.Star),
                MinHeight = 100
            });
            MainGrid.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(SplitterThickness),
                MaxHeight = SplitterThickness
            });
            MainGrid.RowDefinitions.Add(new RowDefinition
            {
                Height = _zoneSizes[DockSide.Bottom],
                MinHeight = 100,
                MaxHeight = double.PositiveInfinity
            });
        }

        private void PlaceChildrenOnce()
        {
            // Zones
            Grid.SetColumn(LeftZone, ColLeft);
            Grid.SetRow(LeftZone, RowCenter);
            Grid.SetRowSpan(LeftZone, 3); // Center row + top/bottom если нет панелей

            Grid.SetColumn(RightZone, ColRight);
            Grid.SetRow(RightZone, RowCenter);
            Grid.SetRowSpan(RightZone, 3);

            Grid.SetColumn(TopZone, ColCenter);
            Grid.SetRow(TopZone, RowTop);
            Grid.SetColumnSpan(TopZone, 3); // Center column + left/right если нет панелей

            Grid.SetColumn(BottomZone, ColCenter);
            Grid.SetRow(BottomZone, RowBottom);
            Grid.SetColumnSpan(BottomZone, 3);

            // Center
            Grid.SetColumn(CenterFrame, ColCenter);
            Grid.SetRow(CenterFrame, RowCenter);

            // Splitters
            Grid.SetColumn(LeftSplitter, ColLeftSplit);
            Grid.SetRow(LeftSplitter, RowCenter);
            Grid.SetRowSpan(LeftSplitter, 3);

            Grid.SetColumn(RightSplitter, ColRightSplit);
            Grid.SetRow(RightSplitter, RowCenter);
            Grid.SetRowSpan(RightSplitter, 3);

            Grid.SetColumn(TopSplitter, ColCenter);
            Grid.SetRow(TopSplitter, RowTopSplit);
            Grid.SetColumnSpan(TopSplitter, 3);

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

            // Вычисляем spans для центральной области
            int contentCol = hasLeft ? ColCenter : ColLeft;
            int contentColSpan = 1 + (hasLeft ? 0 : 2) + (hasRight ? 0 : 2);

            int contentRow = hasTop ? RowCenter : RowTop;
            int contentRowSpan = 1 + (hasTop ? 0 : 2) + (hasBottom ? 0 : 2);

            // Обновляем позиции и spans
            Grid.SetColumn(CenterFrame, contentCol);
            Grid.SetColumnSpan(CenterFrame, contentColSpan);
            Grid.SetRow(CenterFrame, contentRow);
            Grid.SetRowSpan(CenterFrame, contentRowSpan);

            // Обновляем spans для верхней/нижней зон
            Grid.SetColumn(TopZone, contentCol);
            Grid.SetColumnSpan(TopZone, contentColSpan);

            Grid.SetColumn(TopSplitter, contentCol);
            Grid.SetColumnSpan(TopSplitter, contentColSpan);

            Grid.SetColumn(BottomZone, contentCol);
            Grid.SetColumnSpan(BottomZone, contentColSpan);

            Grid.SetColumn(BottomSplitter, contentCol);
            Grid.SetColumnSpan(BottomSplitter, contentColSpan);

            // Обновляем spans для левой/правой зон
            Grid.SetRow(LeftZone, contentRow);
            Grid.SetRowSpan(LeftZone, contentRowSpan);

            Grid.SetRow(LeftSplitter, contentRow);
            Grid.SetRowSpan(LeftSplitter, contentRowSpan);

            Grid.SetRow(RightZone, contentRow);
            Grid.SetRowSpan(RightZone, contentRowSpan);

            Grid.SetRow(RightSplitter, contentRow);
            Grid.SetRowSpan(RightSplitter, contentRowSpan);

            // Устанавливаем видимость
            TopZone.Visibility = hasTop ? Visibility.Visible : Visibility.Collapsed;
            TopSplitter.Visibility = hasTop ? Visibility.Visible : Visibility.Collapsed;

            BottomZone.Visibility = hasBottom ? Visibility.Visible : Visibility.Collapsed;
            BottomSplitter.Visibility = hasBottom ? Visibility.Visible : Visibility.Collapsed;

            LeftZone.Visibility = hasLeft ? Visibility.Visible : Visibility.Collapsed;
            LeftSplitter.Visibility = hasLeft ? Visibility.Visible : Visibility.Collapsed;

            RightZone.Visibility = hasRight ? Visibility.Visible : Visibility.Collapsed;
            RightSplitter.Visibility = hasRight ? Visibility.Visible : Visibility.Collapsed;

            // Обновляем размеры зон
            if (MainGrid.ColumnDefinitions.Count == 5 && MainGrid.RowDefinitions.Count == 5)
            {
                // Левая зона
                if (hasLeft)
                {
                    // Используем сохраненный размер, если он есть, иначе дефолтный
                    var size = _lastValidSizes.TryGetValue(DockSide.Left, out var validSize)
                        ? validSize
                        : _zoneSizes[DockSide.Left];

                    MainGrid.ColumnDefinitions[ColLeft].Width = size;
                    MainGrid.ColumnDefinitions[ColLeftSplit].Width = new GridLength(SplitterThickness);
                }
                else
                {
                    MainGrid.ColumnDefinitions[ColLeft].Width = new GridLength(0);
                    MainGrid.ColumnDefinitions[ColLeftSplit].Width = new GridLength(0);
                }

                // Правая зона
                if (hasRight)
                {
                    var size = _lastValidSizes.TryGetValue(DockSide.Right, out var validSize)
                        ? validSize
                        : _zoneSizes[DockSide.Right];

                    MainGrid.ColumnDefinitions[ColRight].Width = size;
                    MainGrid.ColumnDefinitions[ColRightSplit].Width = new GridLength(SplitterThickness);
                }
                else
                {
                    MainGrid.ColumnDefinitions[ColRight].Width = new GridLength(0);
                    MainGrid.ColumnDefinitions[ColRightSplit].Width = new GridLength(0);
                }

                // Верхняя зона
                if (hasTop)
                {
                    var size = _lastValidSizes.TryGetValue(DockSide.Top, out var validSize)
                        ? validSize
                        : _zoneSizes[DockSide.Top];

                    MainGrid.RowDefinitions[RowTop].Height = size;
                    MainGrid.RowDefinitions[RowTopSplit].Height = new GridLength(SplitterThickness);
                }
                else
                {
                    MainGrid.RowDefinitions[RowTop].Height = new GridLength(0);
                    MainGrid.RowDefinitions[RowTopSplit].Height = new GridLength(0);
                }

                // Нижняя зона
                if (hasBottom)
                {
                    var size = _lastValidSizes.TryGetValue(DockSide.Bottom, out var validSize)
                        ? validSize
                        : _zoneSizes[DockSide.Bottom];

                    MainGrid.RowDefinitions[RowBottom].Height = size;
                    MainGrid.RowDefinitions[RowBottomSplit].Height = new GridLength(SplitterThickness);
                }
                else
                {
                    MainGrid.RowDefinitions[RowBottom].Height = new GridLength(0);
                    MainGrid.RowDefinitions[RowBottomSplit].Height = new GridLength(0);
                }
            }

          
        }

        public void AddPanel(DockPanelInfo panel, DockSide side)
        {
            if (panel == null) return;

            if (_zones.TryGetValue(side, out var zone))
            {
                // Удаляем панель из всех зон перед добавлением
                RemovePanel(panel);

                zone.AddPanel(panel);

                // Подписываемся на событие закрытия
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