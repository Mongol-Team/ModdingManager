using Application.Debugging;
using Application.Extentions;
using Application.utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Controls
{
    public enum PanelSide
    {
        Left, Right
    }

    public partial class WindowTitleBar : UserControl
    {
        private StackPanel _leftPanel;
        private StackPanel _rightPanel;
        private List<Tuple<Button, PanelSide>> _pendingButtons = new List<Tuple<Button, PanelSide>>();
        private bool _isInitialized = false;
        private Window _parentWindow;
        private bool _isDragging = false;
        private Point _clickPosition;
        public WindowTitleBar()
        {
            InitializeComponent();
            this.Loaded += OnWindowTitleBarLoaded;
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            MouseMove += OnMouseMove;
        }

        private void OnWindowTitleBarLoaded(object sender, RoutedEventArgs e)
        {
            InitializePanels();

            // Добавляем все отложенные кнопки
            foreach (var pendingButton in _pendingButtons)
            {
                AddButtonInternal(pendingButton.Item1, pendingButton.Item2);
            }
            _pendingButtons.Clear();

            Logger.AddLog("WindowTitleBar полностью загружен и инициализирован");
            _parentWindow = Window.GetWindow(this);

            if (_parentWindow == null)
            {
                Logger.AddLog(StaticLocalisation.GetString("Log.WindowTitleBar.ParentWindowNotFound"));
            }
            else
            {
                Logger.AddDbgLog(StaticLocalisation.GetString("Log.WindowTitleBar.ParentWindowFound", _parentWindow.Title));
            }
        }

        private void InitializePanels()
        {
            // Получаем ссылки на панели из XAML
            _leftPanel = LeftPanel;
            _rightPanel = RightPanel;

            if (_leftPanel == null || _rightPanel == null)
            {
                Logger.AddLog("Ошибка: Панели не найдены в XAML");
                return;
            }

            _isInitialized = true;
            Logger.AddLog($"Панели инициализированы. LeftPanel: {_leftPanel != null}, RightPanel: {_rightPanel != null}");
        }

        public void AddButton(Button button, PanelSide side)
        {
            var stopwatch = Stopwatch.StartNew();

            // Если панели еще не инициализированы, сохраняем кнопку для отложенного добавления
            if (!_isInitialized)
            {
                _pendingButtons.Add(new Tuple<Button, PanelSide>(button, side));
                Logger.AddLog($"Кнопка '{button.Content}' отложена для добавления на сторону {side}");
                return;
            }

            // Добавляем кнопку сразу
            AddButtonInternal(button, side);

            stopwatch.Stop();
            Logger.AddLog($"Кнопка '{button.Content}' добавлена на сторону {side}. Время: {stopwatch.ElapsedMilliseconds}мс");
        }

        private void AddButtonInternal(Button button, PanelSide side)
        {
            try
            {
                // Устанавливаем свойства кнопки
                button.MinWidth = 70;
                button.Height = 28;
                button.Margin = new Thickness(5, 0, 0, 0);
                button.Padding = new Thickness(8, 4, 8, 4);

                // Применяем стиль, если он есть
                if (button.Style == null)
                {
                    var style = FindResource("TitlebarButtonStyle") as Style;
                    if (style != null)
                    {
                        button.Style = style;
                    }
                }

                StackPanel targetPanel = side == PanelSide.Left ? _leftPanel : _rightPanel;
                if (targetPanel != null)
                {
                    targetPanel.Children.Add(button);
                    Logger.AddDbgLog($"Кнопка '{button.Content}' успешно добавлена в панель");
                }
                else
                {
                    Logger.AddLog($"Ошибка: Панель для стороны {side} не найдена");
                }
            }
            catch (Exception ex)
            {
                Logger.AddLog($"Ошибка при добавлении кнопки '{button.Content}': {ex.Message}");
            }
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    if (e.ClickCount == 2)
                    {
                        // Двойной клик - максимизация/восстановление
                        window.WindowState = window.WindowState == WindowState.Maximized
                            ? WindowState.Normal
                            : WindowState.Maximized;
                    }
                    else
                    {
                        // Одинарный клик - перетаскивание
                        window.DragMove();
                    }
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

        /// <summary>
        /// Получаем ссылку на родительское окно при загрузке контрола
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _parentWindow = Window.GetWindow(this);

            if (_parentWindow == null)
            {
                Logger.AddLog(StaticLocalisation.GetString("Log.WindowTitleBar.ParentWindowNotFound"));
            }
            else
            {
                Logger.AddDbgLog(StaticLocalisation.GetString("Log.WindowTitleBar.ParentWindowFound", _parentWindow.Title));
            }
        }

        /// <summary>
        /// Начало перетаскивания окна
        /// </summary>
        /// <summary>
        /// Обработка нажатия левой кнопки мыши для перемещения окна
        /// </summary>
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           

            try
            {
                if (_parentWindow.WindowState == WindowState.Maximized)
                {
                    Point mousePosition = e.GetPosition(_parentWindow);
                    double mouseRelativeX = mousePosition.X / _parentWindow.ActualWidth;

                    _parentWindow.WindowState = WindowState.Normal;

                    Point screenPoint = PointToScreen(e.GetPosition(this));
                    _parentWindow.Left = screenPoint.X - (_parentWindow.ActualWidth * mouseRelativeX);
                    _parentWindow.Top = screenPoint.Y - mousePosition.Y;

                    Logger.AddDbgLog(StaticLocalisation.GetString("Log.WindowTitleBar.WindowRestoredForDrag"));
                }

                // Оптимизация: отключаем сложный рендеринг во время перетаскивания
                DisableRenderingOptimizations();

                _parentWindow.DragMove();

                // Восстанавливаем после завершения перетаскивания
                EnableRenderingOptimizations();

                Logger.AddDbgLog(StaticLocalisation.GetString("Log.WindowTitleBar.WindowDragged"));
            }
            catch (InvalidOperationException ex)
            {
               
            }
        }

        /// <summary>
        /// Отключает визуальные эффекты для оптимизации производительности
        /// </summary>
        private void DisableRenderingOptimizations()
        {
            if (_parentWindow == null) return;

            // Сохраняем текущее значение AllowsTransparency (если используется)
            // И временно отключаем аппаратное ускорение для сложных элементов
            System.Windows.Media.RenderOptions.SetBitmapScalingMode(_parentWindow, System.Windows.Media.BitmapScalingMode.LowQuality);

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.WindowTitleBar.RenderOptimizationsDisabled"));
        }

        /// <summary>
        /// Восстанавливает визуальные эффекты после перетаскивания
        /// </summary>
        private void EnableRenderingOptimizations()
        {
            if (_parentWindow == null) return;

            System.Windows.Media.RenderOptions.SetBitmapScalingMode(_parentWindow, System.Windows.Media.BitmapScalingMode.HighQuality);

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.WindowTitleBar.RenderOptimizationsEnabled"));
        }



        /// <summary>
        /// Завершение перетаскивания окна
        /// </summary>
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                ReleaseMouseCapture();

                Logger.AddDbgLog(StaticLocalisation.GetString("Log.WindowTitleBar.DragEnded"));
            }
        }

        /// <summary>
        /// Перемещение окна при движении мыши
        /// </summary>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || _parentWindow == null)
            {
                return;
            }

            Point currentPosition = e.GetPosition(_parentWindow);
            Point screenPosition = PointToScreen(currentPosition);

            // Вычисляем новую позицию окна
            double newLeft = screenPosition.X - _clickPosition.X;
            double newTop = screenPosition.Y - _clickPosition.Y;

            _parentWindow.Left = newLeft;
            _parentWindow.Top = newTop;
        }

        /// <summary>
        /// Проверка, был ли клик по кнопке управления окном
        /// </summary>
        private bool IsClickOnButton(object source)
        {
            // Проверяем, не является ли источник события кнопкой или её дочерним элементом
            DependencyObject element = source as DependencyObject;

            while (element != null && element != this)
            {
                if (element is Button)
                {
                    return true;
                }
                element = element.GetVisualParent();
            }

            return false;
        }

        /// <summary>
        /// Обработка двойного клика для максимизации/восстановления окна
        /// </summary>
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (_parentWindow == null || IsClickOnButton(e.OriginalSource))
            {
                return;
            }

            if (_parentWindow.WindowState == WindowState.Maximized)
            {
                _parentWindow.WindowState = WindowState.Normal;
                Logger.AddDbgLog(StaticLocalisation.GetString("Log.WindowTitleBar.WindowRestored"));
            }
            else
            {
                _parentWindow.WindowState = WindowState.Maximized;
                Logger.AddDbgLog(StaticLocalisation.GetString("Log.WindowTitleBar.WindowMaximized"));
            }
        }
    }
}
