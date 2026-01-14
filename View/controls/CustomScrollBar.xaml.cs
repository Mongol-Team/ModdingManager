using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UserControl = System.Windows.Controls.UserControl;

namespace ViewControls
{
    public partial class CustomScrollBar : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(CustomScrollBar),
                new PropertyMetadata(0.0, OnValueChanged));

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(CustomScrollBar),
                new PropertyMetadata(0.0, OnValueChanged));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(CustomScrollBar),
                new PropertyMetadata(100.0, OnValueChanged));

        public static readonly DependencyProperty ViewportSizeProperty =
            DependencyProperty.Register("ViewportSize", typeof(double), typeof(CustomScrollBar),
                new PropertyMetadata(10.0, OnValueChanged));

        public static readonly DependencyProperty SmallChangeProperty =
            DependencyProperty.Register("SmallChange", typeof(double), typeof(CustomScrollBar),
                new PropertyMetadata(1.0));

        public static readonly DependencyProperty LargeChangeProperty =
            DependencyProperty.Register("LargeChange", typeof(double), typeof(CustomScrollBar),
                new PropertyMetadata(10.0));

        private static readonly DependencyProperty ThumbHeightProperty =
            DependencyProperty.Register("ThumbHeight", typeof(double), typeof(CustomScrollBar),
                new PropertyMetadata(20.0));

        private bool _isDragging;
        private System.Windows.Point _lastMousePosition;
        private double _thumbOffset;

        public event EventHandler ValueChanged;

        public CustomScrollBar()
        {
            InitializeComponent();
            Loaded += CustomScrollBar_Loaded;
            SizeChanged += CustomScrollBar_SizeChanged;
        }

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, Math.Max(Minimum, Math.Min(Maximum, value)));
        }

        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public double ViewportSize
        {
            get => (double)GetValue(ViewportSizeProperty);
            set => SetValue(ViewportSizeProperty, value);
        }

        public double SmallChange
        {
            get => (double)GetValue(SmallChangeProperty);
            set => SetValue(SmallChangeProperty, value);
        }

        public double LargeChange
        {
            get => (double)GetValue(LargeChangeProperty);
            set => SetValue(LargeChangeProperty, value);
        }

        private double ThumbHeight
        {
            get => (double)GetValue(ThumbHeightProperty);
            set => SetValue(ThumbHeightProperty, value);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CustomScrollBar scrollBar)
            {
                scrollBar.UpdateThumb();
                scrollBar.ValueChanged?.Invoke(scrollBar, EventArgs.Empty);
            }
        }

        private void CustomScrollBar_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateThumb();
        }

        private void CustomScrollBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateThumb();
        }

        private void UpdateThumb()
        {
            if (ActualHeight <= 0) return;

            var trackHeight = ActualHeight - 24;
            if (trackHeight <= 0) return;

            var range = Maximum - Minimum;
            if (range <= 0) return;

            var viewportRatio = Math.Min(1.0, ViewportSize / range);
            var minThumbHeight = 20.0;
            var calculatedThumbHeight = Math.Max(minThumbHeight, trackHeight * viewportRatio);

            ThumbHeight = calculatedThumbHeight;

            var availableSpace = trackHeight - calculatedThumbHeight;
            if (availableSpace <= 0)
            {
                Canvas.SetTop(ThumbBorder, 0);
                return;
            }

            var normalizedValue = (Value - Minimum) / range;
            normalizedValue = Math.Max(0, Math.Min(1, normalizedValue));

            var thumbTop = availableSpace * normalizedValue;
            Canvas.SetTop(ThumbBorder, thumbTop);
        }

        private void DecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            Value = Math.Max(Minimum, Value - SmallChange);
        }

        private void IncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            Value = Math.Min(Maximum, Value + SmallChange);
        }

        private void TrackBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            var trackHeight = ActualHeight - 24;
            var clickY = e.GetPosition(TrackCanvas).Y;
            var thumbCenter = ThumbHeight / 2;
            var targetThumbTop = clickY - thumbCenter;

            var availableSpace = trackHeight - ThumbHeight;
            if (availableSpace <= 0) return;

            var normalizedPosition = Math.Max(0, Math.Min(1, targetThumbTop / availableSpace));
            var range = Maximum - Minimum;
            Value = Minimum + (normalizedPosition * range);
        }

        private void ThumbBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            _isDragging = true;
            _lastMousePosition = e.GetPosition(this);
            _thumbOffset = Canvas.GetTop(ThumbBorder) - _lastMousePosition.Y;
            ThumbBorder.CaptureMouse();
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!_isDragging) return;

            var currentPosition = e.GetPosition(this);
            var deltaY = currentPosition.Y - _lastMousePosition.Y;
            _lastMousePosition = currentPosition;

            var trackHeight = ActualHeight - 24;
            var availableSpace = trackHeight - ThumbHeight;
            if (availableSpace <= 0) return;

            var newThumbTop = Canvas.GetTop(ThumbBorder) + deltaY;
            newThumbTop = Math.Max(0, Math.Min(availableSpace, newThumbTop));

            Canvas.SetTop(ThumbBorder, newThumbTop);

            var normalizedPosition = newThumbTop / availableSpace;
            var range = Maximum - Minimum;
            Value = Minimum + (normalizedPosition * range);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (_isDragging)
            {
                _isDragging = false;
                ThumbBorder.ReleaseMouseCapture();
            }
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (_isDragging)
            {
                _isDragging = false;
                ThumbBorder.ReleaseMouseCapture();
            }
        }
    }
}

