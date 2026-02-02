using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Diagnostics;
using Application.Debugging;
using System.Collections.Generic;

namespace ViewControls
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

        public WindowTitleBar()
        {
            InitializeComponent();
            this.Loaded += OnWindowTitleBarLoaded;
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
    }
}