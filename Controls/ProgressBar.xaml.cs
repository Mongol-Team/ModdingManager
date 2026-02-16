using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using UserControl = System.Windows.Controls.UserControl;

namespace Controls
{
    public partial class ProgressBar : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(ProgressBar),
                new PropertyMetadata(0.0, OnValueChanged));

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(ProgressBar),
                new PropertyMetadata(0.0, OnValueChanged));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(ProgressBar),
                new PropertyMetadata(100.0, OnValueChanged));

        public static readonly DependencyProperty ProgressBrushProperty =
            DependencyProperty.Register("ProgressBrush", typeof(Brush), typeof(ProgressBar),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0, 122, 204))));

        public static readonly DependencyProperty ShowTextProperty =
            DependencyProperty.Register("ShowText", typeof(bool), typeof(ProgressBar),
                new PropertyMetadata(false));

        public static readonly DependencyProperty DisplayTextProperty =
            DependencyProperty.Register(nameof(DisplayText), typeof(string), typeof(ProgressBar),
                new PropertyMetadata(string.Empty));

        private static readonly DependencyProperty ProgressWidthProperty =
            DependencyProperty.Register("ProgressWidth", typeof(double), typeof(ProgressBar),
                new PropertyMetadata(0.0));

        public ProgressBar()
        {
            InitializeComponent();
            Loaded += ProgressBar_Loaded;
            SizeChanged += ProgressBar_SizeChanged;
        }

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
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

        public Brush ProgressBrush
        {
            get => (Brush)GetValue(ProgressBrushProperty);
            set => SetValue(ProgressBrushProperty, value);
        }

        public bool ShowText
        {
            get => (bool)GetValue(ShowTextProperty);
            set => SetValue(ShowTextProperty, value);
        }

        public string DisplayText
        {
            get => (string)GetValue(DisplayTextProperty);
            set => SetValue(DisplayTextProperty, value);
        }

        public string ProgressText
        {
            get => DisplayText;
            set => DisplayText = value;
        }

        private double ProgressWidth
        {
            get => (double)GetValue(ProgressWidthProperty);
            set => SetValue(ProgressWidthProperty, value);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProgressBar progressBar)
            {
                progressBar.UpdateProgress();
            }
        }

        private void ProgressBar_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateProgress();
        }

        private void ProgressBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateProgress();
        }

        private void UpdateProgress()
        {
            if (ActualWidth <= 0) return;

            var range = Maximum - Minimum;
            if (range <= 0) return;

            var normalizedValue = (Value - Minimum) / range;
            normalizedValue = Math.Max(0, Math.Min(1, normalizedValue));

            ProgressWidth = ActualWidth * normalizedValue;

            if (ShowText && string.IsNullOrEmpty(DisplayText))
            {
                DisplayText = $"{normalizedValue * 100:F0}%";
            }
        }
    }
}

