using Application.Debugging;
using Application.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Controls
{
    /// <summary>
    /// Ориентация ленты файлов.
    /// </summary>
    public enum StripeOrientation
    {
        Horizontal,
        Vertical
    }

    /// <summary>
    /// Данные об одном файл-табе.
    /// </summary>
    public class FileTabItem
    {
        /// <summary>Путь к файлу</summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>Отображаемое имя файла</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Закреплён ли таб</summary>
        public bool IsPinned { get; set; } = false;

        /// <summary>Порядок закрепления (меньше — левее/выше среди закреплённых)</summary>
        public int PinOrder { get; set; } = -1;

        /// <summary>Ссылка на обозреваемый объект (любой тип данных)</summary>
        public object? FileObject { get; set; }

        internal Button? Button { get; set; }
    }

    /// <summary>
    /// Аргументы события файлового таба.
    /// </summary>
    public class FileTabEventArgs : EventArgs
    {
        public FileTabItem Tab { get; }
        public FileTabEventArgs(FileTabItem tab) => Tab = tab;
    }

    /// <summary>
    /// Лента файловых табов — аналог строки вкладок Visual Studio.
    /// Поддерживает горизонтальную и вертикальную ориентацию,
    /// закрепление вкладок, прокрутку кнопками и совместимость с DockZone.
    /// </summary>
    public partial class FilesStripeControl : UserControl
    {
        // ─── События ──────────────────────────────────────────────────────────

        /// <summary>Двойной клик по вкладке или обычный клик — «открыть файл».</summary>
        public event EventHandler<FileTabEventArgs>? FileOpenRequested;

        /// <summary>Активный файл изменился.</summary>
        public event EventHandler<FileTabEventArgs>? ActiveFileChanged;

        /// <summary>Закрытие одной вкладки (крестик).</summary>
        public event EventHandler<FileTabEventArgs>? FileCloseRequested;

        /// <summary>Закрытие всех вкладок.</summary>
        public event EventHandler? AllFilesCloseRequested;

        // ─── Приватные поля ────────────────────────────────────────────────────

        private readonly List<FileTabItem> _tabs = new();
        private int _scrollOffset = 0; // индекс первого видимого таба
        private FileTabItem? _activeTab; // текущий открытый файл

        private StripeOrientation _orientation;
        private StackPanel _activePanel = null!;
        private ScrollViewer _activeScrollViewer = null!;
        private Button _scrollPrevButton = null!;
        private Button _scrollNextButton = null!;
        
        // ─── DependencyProperty: Orientation ──────────────────────────────────

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(StripeOrientation),
                typeof(FilesStripeControl),
                new PropertyMetadata(StripeOrientation.Horizontal, OnOrientationChanged));

        public StripeOrientation Orientation
        {
            get => (StripeOrientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FilesStripeControl ctrl)
                ctrl.ApplyOrientation((StripeOrientation)e.NewValue);
        }

        // ─── Конструктор ──────────────────────────────────────────────────────

        public FilesStripeControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;

        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplyOrientation(Orientation);
            SizeChanged += (_, _) => UpdateScrollButtons();
            UpdateScrollOffset();
            Logger.AddLog(StaticLocalisation.GetString("Log.FilesStripe.Loaded"));
        }

        // ─── Ориентация ───────────────────────────────────────────────────────
        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdateScrollOffset();
            UpdateScrollButtons(); // Оновлюємо кнопки після будь-якої зміни прокрутки
        }
        private void UpdateScrollOffset()
        {
            if (_activePanel.Children.Count == 0)
            {
                _scrollOffset = 0;
                return;
            }

            double offset = _orientation == StripeOrientation.Horizontal
                ? _activeScrollViewer.HorizontalOffset
                : _activeScrollViewer.VerticalOffset;

            int index = 0;
            foreach (var child in _activePanel.Children.OfType<FrameworkElement>())
            {
                var pos = child.TransformToAncestor(_activeScrollViewer).Transform(new Point(0, 0));
                double childPos = _orientation == StripeOrientation.Horizontal ? pos.X : pos.Y;

                // Знаходимо перший таб, чия позиція >= поточного offset (перший видимий або частково видимий)
                if (childPos >= offset - 1) // -1 для tolerance на fractional offsets
                {
                    _scrollOffset = index;
                    break;
                }
                index++;
            }

            // Якщо не знайшли (крайній випадок), скидаємо на останній
            if (index >= _activePanel.Children.Count)
                _scrollOffset = _activePanel.Children.Count - 1;

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.FilesStripe.ScrollOffsetUpdated", _scrollOffset));
        }
        private void ApplyOrientation(StripeOrientation orientation)
        {
            _orientation = orientation;

            if (orientation == StripeOrientation.Horizontal)
            {
                HorizontalLayout.Visibility = Visibility.Visible;
                VerticalLayout.Visibility = Visibility.Collapsed;

                _activePanel = HItemsPanel;
                _activeScrollViewer = HScrollViewer;
                _scrollPrevButton = ScrollLeftButton;
                _scrollNextButton = ScrollRightButton;
            }
            else
            {
                HorizontalLayout.Visibility = Visibility.Collapsed;
                VerticalLayout.Visibility = Visibility.Visible;

                _activePanel = VItemsPanel;
                _activeScrollViewer = VScrollViewer;
                _scrollPrevButton = ScrollUpButton;
                _scrollNextButton = ScrollDownButton;
            }
            _activeScrollViewer.ScrollChanged += OnScrollChanged;
            RebuildPanel();
            Logger.AddDbgLog(StaticLocalisation.GetString("Log.FilesStripe.OrientationApplied", orientation));
        }

        // ─── Публичные методы ─────────────────────────────────────────────────

        /// <summary>Добавить вкладку файла.</summary>
        /// <param name="filePath">Путь к файлу</param>
        /// <param name="displayName">Отображаемое имя</param>
        /// <param name="fileObject">Объект, представляющий файл (опционально)</param>
        /// <param name="makeActive">Сделать активным после добавления</param>
        public void AddTab(string filePath, string displayName, object? fileObject = null, bool makeActive = false)
        {
            if (_tabs.Any(t => t.FilePath == filePath))
            {
                Logger.AddDbgLog(StaticLocalisation.GetString("Log.FilesStripe.TabAlreadyExists", filePath));

                // Если таб уже существует и нужно сделать активным
                if (makeActive)
                {
                    var existingTab = _tabs.First(t => t.FilePath == filePath);
                    SetActiveTab(existingTab);
                }
                return;
            }

            var tab = new FileTabItem
            {
                FilePath = filePath,
                DisplayName = displayName,
                FileObject = fileObject
            };

            _tabs.Add(tab);
            Logger.AddLog(StaticLocalisation.GetString("Log.FilesStripe.TabAdded", displayName));
            RebuildPanel();

            if (makeActive)
            {
                SetActiveTab(tab);
            }
        }

        /// <summary>Удалить вкладку по пути файла.</summary>
        public void RemoveTab(string filePath)
        {
            var tab = _tabs.FirstOrDefault(t => t.FilePath == filePath);
            if (tab == null) return;

            // Если удаляется активный таб, нужно выбрать новый активный
            bool wasActive = tab == _activeTab;

            _tabs.Remove(tab);
            Logger.AddLog(StaticLocalisation.GetString("Log.FilesStripe.TabRemoved", tab.DisplayName));

            if (wasActive)
            {
                // Переключаемся на следующий таб или предыдущий
                _activeTab = _tabs.FirstOrDefault();
                if (_activeTab != null)
                {
                    ActiveFileChanged?.Invoke(this, new FileTabEventArgs(_activeTab));
                    Logger.AddDbgLog(StaticLocalisation.GetString("Log.FilesStripe.ActiveFileChanged", _activeTab.FilePath));
                }
            }

            RebuildPanel();
        }

        /// <summary>Закрепить/открепить вкладку.</summary>
        public void SetPinned(string filePath, bool pinned)
        {
            var tab = _tabs.FirstOrDefault(t => t.FilePath == filePath);
            if (tab == null) return;

            tab.IsPinned = pinned;

            if (pinned)
            {
                // Порядок закрепления — по времени вызова
                tab.PinOrder = _tabs.Where(t => t.IsPinned).Max(t => t.PinOrder < 0 ? 0 : t.PinOrder) + 1;
                Logger.AddLog(StaticLocalisation.GetString("Log.FilesStripe.TabPinned", tab.DisplayName));
            }
            else
            {
                tab.PinOrder = -1;
                Logger.AddLog(StaticLocalisation.GetString("Log.FilesStripe.TabUnpinned", tab.DisplayName));
            }

            RebuildPanel();
        }

        /// <summary>Закрыть все вкладки.</summary>
        public void CloseAll()
        {
            _tabs.Clear();
            _scrollOffset = 0;
            _activeTab = null;
            RebuildPanel();
            AllFilesCloseRequested?.Invoke(this, EventArgs.Empty);
            Logger.AddLog(StaticLocalisation.GetString("Log.FilesStripe.AllClosed"));
        }

        /// <summary>Установить активный файл.</summary>
        public void SetActiveTab(string filePath)
        {
            var tab = _tabs.FirstOrDefault(t => t.FilePath == filePath);
            if (tab != null)
            {
                SetActiveTab(tab);
            }
        }

        /// <summary>Получить текущий активный таб.</summary>
        public FileTabItem? GetActiveTab() => _activeTab;

        /// <summary>Получить все табы.</summary>
        public IReadOnlyList<FileTabItem> GetAllTabs() => _tabs.AsReadOnly();

        /// <summary>Получить таб по пути файла.</summary>
        public FileTabItem? GetTab(string filePath) => _tabs.FirstOrDefault(t => t.FilePath == filePath);

        // ─── Приватные методы для работы с активным табом ─────────────────────

        private void SetActiveTab(FileTabItem tab)
        {
            if (_activeTab == tab) return;

            _activeTab = tab;
            RebuildPanel(); // Перерисовываем для обновления визуального состояния
            ActiveFileChanged?.Invoke(this, new FileTabEventArgs(tab));
            Logger.AddDbgLog(StaticLocalisation.GetString("Log.FilesStripe.ActiveFileChanged", tab.FilePath));
        }

        // ─── Построение панели ────────────────────────────────────────────────

        /// <summary>
        /// Полная перестройка визуального состояния панели с учётом порядка:
        /// сначала закреплённые (по PinOrder), затем остальные.
        /// </summary>
        private void RebuildPanel()
        {
            if (_activePanel == null) return;

            var sw = Stopwatch.StartNew();

            _activePanel.Children.Clear();

            // Порядок отображения: закреплённые → остальные
            var ordered = _tabs
                .Where(t => t.IsPinned)
                .OrderBy(t => t.PinOrder)
                .Concat(_tabs.Where(t => !t.IsPinned))
                .ToList();

            for (int i = 0; i < ordered.Count; i++)
            {
                var tab = ordered[i];
                tab.Button = BuildTabButton(tab);
                _activePanel.Children.Add(tab.Button);
            }
            UpdateScrollOffset();
            UpdateScrollButtons();
            sw.Stop();
            Logger.AddLog(StaticLocalisation.GetString("Log.FilesStripe.Rebuilt", ordered.Count, sw.ElapsedMilliseconds));

            UpdateScrollButtons();
        }

        private Button BuildTabButton(FileTabItem tab)
        {
            var nameBlock = new TextBlock
            {
                Text = (tab.IsPinned ? "📌  " : "") + tab.DisplayName,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                // MaxWidth = 140  ← краще прибрати або зробити більшим
            };

            var pinButton = new TextBlock
            {
                Text = tab.IsPinned ? "📌" : "📍",
                FontSize = 12,
                Foreground = Brushes.White,           // ← тимчасово яскравий колір для дебагу
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(6, 0, 6, 0),
                Cursor = Cursors.Hand,
                ToolTip = tab.IsPinned ? "Відкріпити" : "Закріпити",
                Visibility = Visibility.Collapsed,
            };

            var closeButton = new TextBlock
            {
                Text = "✕",
                FontSize = 13,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 10, 0),
                Cursor = Cursors.Hand,
                ToolTip = "Закрити",
                Visibility = Visibility.Collapsed,
            };

            var contentGrid = new Grid { Background = Brushes.Transparent };
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Grid.SetColumn(nameBlock, 0);
            Grid.SetColumn(pinButton, 1);
            Grid.SetColumn(closeButton, 2);

            contentGrid.Children.Add(nameBlock);
            contentGrid.Children.Add(pinButton);
            contentGrid.Children.Add(closeButton);

            var btn = new Button
            {
                Content = contentGrid,
                Style = (Style)FindResource("FileTabButton"),
                Height = _orientation == StripeOrientation.Horizontal ? 32 : double.NaN,
                Padding = new Thickness(10, 0, 4, 0),   // зменшити правий padding
                MinWidth = 100,
                Tag = tab
            };

            // Дебаг: видно, чи спрацьовує подія
            btn.MouseEnter += (_, _) =>
            {
                pinButton.Visibility = Visibility.Visible;
                closeButton.Visibility = Visibility.Visible;
            };
            btn.Click += (_, e) =>
            {
                e.Handled = true;
                SetActiveTab(tab);
                FileOpenRequested?.Invoke(this, new FileTabEventArgs(tab));
            };
            btn.MouseLeave += (_, _) =>
            {
                pinButton.Visibility = Visibility.Collapsed;
                closeButton.Visibility = Visibility.Collapsed;
            };

            pinButton.MouseLeftButtonDown += (_, e) =>
            {
                e.Handled = true;
                SetPinned(tab.FilePath, !tab.IsPinned);
                nameBlock.Text = (tab.IsPinned ? "📌  " : "") + tab.DisplayName;
                pinButton.Text = tab.IsPinned ? "📌" : "📍";
                pinButton.Foreground = tab.IsPinned ? Brushes.Gold : Brushes.Gray;
            };

            closeButton.MouseLeftButtonDown += (_, e) =>
            {
                e.Handled = true;
                OnCloseTabClicked(tab);
            };

            UpdateTabButtonVisualState(btn, tab == _activeTab);

            return btn;
        }

        /// <summary>Обновить визуальное состояние кнопки таба (активный/неактивный).</summary>
        private void UpdateTabButtonVisualState(Button btn, bool isActive)
        {
            if (isActive)
            {
                // Активный таб — яркий фон
                btn.Background = (Brush)FindResource("AccentBrush");
                btn.Foreground = (Brush)FindResource("TextPrimary");
                btn.FontWeight = FontWeights.SemiBold;
            }
            else
            {
                // Неактивный таб — стандартный стиль
                btn.ClearValue(BackgroundProperty);
                btn.ClearValue(ForegroundProperty);
                btn.FontWeight = FontWeights.Normal;
            }
        }

        private void OnCloseTabClicked(FileTabItem tab)
        {
            Logger.AddDbgLog(StaticLocalisation.GetString("Log.FilesStripe.FileCloseRequested", tab.FilePath));
            FileCloseRequested?.Invoke(this, new FileTabEventArgs(tab));
            RemoveTab(tab.FilePath);
        }

        // ─── Скролл ───────────────────────────────────────────────────────────

        private void ItemsPanel_SizeChanged(object sender, SizeChangedEventArgs e)
            => UpdateScrollButtons();

        private void UpdateScrollButtons()
        {
            if (_activeScrollViewer == null || _activePanel == null) return;

            double extent = _orientation == StripeOrientation.Horizontal
                ? _activeScrollViewer.ExtentWidth
                : _activeScrollViewer.ExtentHeight;

            double viewport = _orientation == StripeOrientation.Horizontal
                ? _activeScrollViewer.ViewportWidth
                : _activeScrollViewer.ViewportHeight;

            double offset = _orientation == StripeOrientation.Horizontal
                ? _activeScrollViewer.HorizontalOffset
                : _activeScrollViewer.VerticalOffset;

            bool needScroll = extent > viewport;
            var vis = needScroll ? Visibility.Visible : Visibility.Collapsed;
            _scrollPrevButton.Visibility = vis;
            _scrollNextButton.Visibility = vis;

            if (needScroll)
            {
                _scrollPrevButton.IsEnabled = offset > 0; // Зміна: не _scrollOffset > 0, а реальний offset
                _scrollNextButton.IsEnabled = offset + viewport < extent; // Чи є ще контент за видимою зоною
            }
        }

        /// <summary>Прокручивает ленту к элементу с индексом <paramref name="index"/>.</summary>
        private void ScrollToIndex(int index)
        {
            if (_activePanel.Children.Count == 0) return;
            index = Math.Max(0, Math.Min(index, _activePanel.Children.Count - 1));

            var child = _activePanel.Children[index] as FrameworkElement;
            if (child == null) return;

            var pos = child.TransformToAncestor(_activeScrollViewer).Transform(new Point(0, 0));

            if (_orientation == StripeOrientation.Horizontal)
            {
                _activeScrollViewer.ScrollToHorizontalOffset(pos.X);
            }
            else
            {
                _activeScrollViewer.ScrollToVerticalOffset(pos.Y);
            }

            // _scrollOffset оновиться автоматично через OnScrollChanged
            Logger.AddDbgLog(StaticLocalisation.GetString("Log.FilesStripe.ScrolledTo", index));
        }

        private void ScrollLeftButton_Click(object sender, RoutedEventArgs e)
            => ScrollToIndex(_scrollOffset - 1);

        private void ScrollRightButton_Click(object sender, RoutedEventArgs e)
            => ScrollToIndex(_scrollOffset + 1);

        private void ScrollUpButton_Click(object sender, RoutedEventArgs e)
            => ScrollToIndex(_scrollOffset - 1);

        private void ScrollDownButton_Click(object sender, RoutedEventArgs e)
            => ScrollToIndex(_scrollOffset + 1);
    }
}