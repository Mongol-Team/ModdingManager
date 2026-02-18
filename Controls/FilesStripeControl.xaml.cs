using Application.Debugging;
using Application.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        public string FilePath { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsPinned { get; set; } = false;
        /// <summary>Порядок закрепления (меньше — левее/выше среди закреплённых)</summary>
        public int PinOrder { get; set; } = -1;

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

        /// <summary>Двойной клик по вкладке — «открыть файл».</summary>
        public event EventHandler<FileTabEventArgs>? FileOpenRequested;

        /// <summary>Закрытие одной вкладки (крестик).</summary>
        public event EventHandler<FileTabEventArgs>? FileCloseRequested;

        /// <summary>Закрытие всех вкладок.</summary>
        public event EventHandler? AllFilesCloseRequested;

        // ─── Приватные поля ────────────────────────────────────────────────────

        private readonly List<FileTabItem> _tabs = new();
        private int _scrollOffset = 0; // индекс первого видимого таба

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
            Logger.AddLog(StaticLocalisation.GetString("Log.FilesStripe.Loaded"));
        }

        // ─── Ориентация ───────────────────────────────────────────────────────

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

            RebuildPanel();
            Logger.AddDbgLog(StaticLocalisation.GetString("Log.FilesStripe.OrientationApplied", orientation));
        }

        // ─── Публичные методы ─────────────────────────────────────────────────

        /// <summary>Добавить вкладку файла.</summary>
        public void AddTab(string filePath, string displayName)
        {
            if (_tabs.Any(t => t.FilePath == filePath))
            {
                Logger.AddDbgLog(StaticLocalisation.GetString("Log.FilesStripe.TabAlreadyExists", filePath));
                return;
            }

            var tab = new FileTabItem
            {
                FilePath = filePath,
                DisplayName = displayName
            };

            _tabs.Add(tab);
            Logger.AddLog(StaticLocalisation.GetString("Log.FilesStripe.TabAdded", displayName));
            RebuildPanel();
        }

        /// <summary>Удалить вкладку по пути файла.</summary>
        public void RemoveTab(string filePath)
        {
            var tab = _tabs.FirstOrDefault(t => t.FilePath == filePath);
            if (tab == null) return;

            _tabs.Remove(tab);
            Logger.AddLog(StaticLocalisation.GetString("Log.FilesStripe.TabRemoved", tab.DisplayName));
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
            RebuildPanel();
            AllFilesCloseRequested?.Invoke(this, EventArgs.Empty);
            Logger.AddLog(StaticLocalisation.GetString("Log.FilesStripe.AllClosed"));
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

            sw.Stop();
            Logger.AddLog(StaticLocalisation.GetString("Log.FilesStripe.Rebuilt", ordered.Count, sw.ElapsedMilliseconds));

            UpdateScrollButtons();
        }

        private Button BuildTabButton(FileTabItem tab)
        {
            // Контент: [📌 если закреплён] Имя  [✕]
            var nameBlock = new TextBlock
            {
                Text = (tab.IsPinned ? "📌 " : string.Empty) + tab.DisplayName,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                MaxWidth = 150
            };

            var closeBlock = new TextBlock
            {
                Text = "✕",
                FontSize = 10,
                Foreground = (System.Windows.Media.Brush)FindResource("TextTertiary"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(6, 0, 0, 0),
                Visibility = Visibility.Collapsed,
                Cursor = Cursors.Hand
            };

            var contentGrid = new Grid();
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(nameBlock, 0);
            Grid.SetColumn(closeBlock, 1);
            contentGrid.Children.Add(nameBlock);
            contentGrid.Children.Add(closeBlock);

            var btn = new Button
            {
                Content = contentGrid,
                Style = (Style)FindResource("FileTabButton"),
                Height = _orientation == StripeOrientation.Horizontal ? 30 : double.NaN,
                Width = _orientation == StripeOrientation.Vertical ? double.NaN : double.NaN,
                Margin = _orientation == StripeOrientation.Horizontal
                    ? new Thickness(0, 0, 0, 0)
                    : new Thickness(0, 0, 0, 1),
                Padding = new Thickness(10, 0, 6, 0),
                ToolTip = tab.FilePath
            };

            // Показ крестика при наведении
            btn.MouseEnter += (_, _) => closeBlock.Visibility = Visibility.Visible;
            btn.MouseLeave += (_, _) => closeBlock.Visibility = Visibility.Collapsed;

            // Двойной клик — открыть файл
            btn.MouseDoubleClick += (_, e) =>
            {
                e.Handled = true;
                Logger.AddDbgLog(StaticLocalisation.GetString("Log.FilesStripe.FileOpenRequested", tab.FilePath));
                FileOpenRequested?.Invoke(this, new FileTabEventArgs(tab));
            };

            // Клик по крестику (через нажатие на closeBlock внутри кнопки)
            closeBlock.MouseLeftButtonDown += (_, e) =>
            {
                e.Handled = true;
                OnCloseTabClicked(tab);
            };

            return btn;
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

            bool needScroll = _orientation == StripeOrientation.Horizontal
                ? _activePanel.ActualWidth > _activeScrollViewer.ActualWidth
                : _activePanel.ActualHeight > _activeScrollViewer.ActualHeight;

            var vis = needScroll ? Visibility.Visible : Visibility.Collapsed;
            _scrollPrevButton.Visibility = vis;
            _scrollNextButton.Visibility = vis;

            if (needScroll)
            {
                // Обновляем доступность кнопок прокрутки
                _scrollPrevButton.IsEnabled = _scrollOffset > 0;
                _scrollNextButton.IsEnabled = _scrollOffset < _activePanel.Children.Count - 1;
            }
        }

        /// <summary>Прокручивает ленту к элементу с индексом <paramref name="index"/>.</summary>
        private void ScrollToIndex(int index)
        {
            if (_activePanel.Children.Count == 0) return;

            index = Math.Max(0, Math.Min(index, _activePanel.Children.Count - 1));
            _scrollOffset = index;

            var child = _activePanel.Children[index] as FrameworkElement;
            if (child == null) return;

            child.BringIntoView();
            UpdateScrollButtons();

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