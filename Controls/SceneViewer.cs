using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Cursors = System.Windows.Input.Cursors;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;

namespace Controls
{
    public class SceneViewer : ScrollViewer
    {
        private Point? _dragStart;
        private Point _scrollStart;
        private bool _isDragging;
        private double _currentZoom = 1.0;
        private FrameworkElement _content;
        private ScaleTransform _layoutTransform = new ScaleTransform(1.0, 1.0);

        public double MaxZoom => 10.0;
        public double MinZoom => 0.1;

        public double CurrentZoom
        {
            get => _currentZoom;
            set => ApplyZoom(value, new Point(ActualWidth / 2, ActualHeight / 2));
        }

        public event EventHandler<double> ZoomChanged;

        public SceneViewer()
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            PreviewMouseMove += OnMouseMove;
            PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            PreviewMouseWheel += OnMouseWheel;
            PreviewMouseRightButtonDown += OnPreviewMouseRightButtonDown;
            Loaded += OnLoaded;
        }
        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_content == null) return;

            // Получаем позицию относительно контента
            Point position = e.GetPosition(_content);

            // Создаём новое событие
            var args = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton)
            {
                RoutedEvent = UIElement.MouseRightButtonDownEvent,
                Source = e.Source
            };

            // Устанавливаем позицию (важно, чтобы было относительно _content)
            _content.RaiseEvent(args);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _content = Content as FrameworkElement;
            if (_content != null)
            {
                _content.LayoutTransform = _layoutTransform;
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && _content != null)
            {
                // Создаём новое событие для контента
                var args = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
                {
                    RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                    Source = e.Source
                };

                _content.RaiseEvent(args);
                return; // двойной клик — не начинаем drag
            }

            if (e.ChangedButton == MouseButton.Left && !_isDragging)
            {
                _dragStart = e.GetPosition(this);
                _scrollStart = new Point(HorizontalOffset, VerticalOffset);
                Cursor = Cursors.Hand;
                CaptureMouse();
                e.Handled = true;
                _isDragging = true;
            }
        }


        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _dragStart.HasValue)
            {
                Point current = e.GetPosition(this);
                Vector delta = _dragStart.Value - current;

                ScrollToHorizontalOffset(_scrollStart.X + delta.X);
                ScrollToVerticalOffset(_scrollStart.Y + delta.Y);
                this.InvalidateMeasure();
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _dragStart = null;
                Cursor = Cursors.Arrow;
                ReleaseMouseCapture();
                _isDragging = false;
                this.InvalidateMeasure();
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            bool shiftHeld = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;

            if (shiftHeld)
            {
                Point zoomCenter = e.GetPosition(_content ?? this);
                double zoomFactor = e.Delta > 0 ? 1.1 : 1 / 1.1;
                double newZoom = Math.Max(MinZoom, Math.Min(MaxZoom, _currentZoom * zoomFactor));
                ApplyZoom(newZoom, zoomCenter);
                e.Handled = true;
            }
            else
            {
                bool hasParent = VisualTreeHelper.GetParent(this) != null;
                if (hasParent)
                {
                    e.Handled = false;
                }
                else
                {
                    ScrollToVerticalOffset(VerticalOffset - e.Delta);
                    e.Handled = true;
                }
            }
        }

        public void SetViewCenter(Point contentCoord)
        {
            if (_content == null)
                return;

            double scaledX = contentCoord.X * _currentZoom;
            double scaledY = contentCoord.Y * _currentZoom;

            double targetOffsetX = scaledX - ActualWidth / 2;
            double targetOffsetY = scaledY - ActualHeight / 2;

            ScrollToHorizontalOffset(Math.Max(0, targetOffsetX));
            ScrollToVerticalOffset(Math.Max(0, targetOffsetY));
        }

        private void ApplyZoom(double newZoom, Point zoomCenter)
        {
            if (Math.Abs(_currentZoom - newZoom) < 0.0001 || _content == null)
                return;

            _layoutTransform.ScaleX = newZoom;
            _layoutTransform.ScaleY = newZoom;
            _currentZoom = newZoom;

            UpdateLayout();

            double offsetX = zoomCenter.X * newZoom - ActualWidth / 2;
            double offsetY = zoomCenter.Y * newZoom - ActualHeight / 2;

            ScrollToHorizontalOffset(Math.Max(0, offsetX));
            ScrollToVerticalOffset(Math.Max(0, offsetY));

            ZoomChanged?.Invoke(this, newZoom);
        }
    }
}
