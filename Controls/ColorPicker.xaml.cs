using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Color = System.Windows.Media.Color;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using UserControl = System.Windows.Controls.UserControl;

namespace Controls
{
    /// <summary>
    /// Логика взаимодействия для ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        private WriteableBitmap _bitmap;

        // DP для SelectedColor
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(nameof(SelectedColor), typeof(Color),
                typeof(ColorPicker), new PropertyMetadata(Colors.White, OnSelectedColorChanged));

        // Событие
        public event RoutedPropertyChangedEventHandler<Color> SelectedColorChanged;

        public Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public ColorPicker()
        {
            InitializeComponent();
            Loaded += ColorPicker_Loaded;
            MouseMove += ColorImage_MouseMove;
        }
        public void SetSelectorPos()
        {
            if (_bitmap == null || PART_ColorImage == null) return;

            var (hue, saturation, _) = RgbToHsv(SelectedColor.R, SelectedColor.G, SelectedColor.B);

            double x = hue / 360.0 * PART_ColorImage.ActualWidth;
            double y = (1 - saturation) * PART_ColorImage.ActualHeight;

            Canvas.SetLeft(PART_Selector, x - PART_Selector.Width / 2);
            Canvas.SetTop(PART_Selector, y - PART_Selector.Height / 2);
            PART_Selector.Visibility = Visibility.Visible;
        }

        private void OnPopupContentMouseDown(object sender, MouseButtonEventArgs e)
        {
            var source = e.OriginalSource as DependencyObject;

            if (IsElementWithin(source, PART_ColorImage))
                return;

            if (IsElementWithin(source, PART_R) ||
                IsElementWithin(source, PART_G) ||
                IsElementWithin(source, PART_B))
                return;

            e.Handled = true;
        }
        private void ColorImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                PickColor(e.GetPosition(PART_ColorImage));
            }
        }
        // Рекурсивный поиск вверх по визуальному дереву
        private bool IsElementWithin(DependencyObject? source, FrameworkElement target)
        {
            while (source != null)
            {
                if (source == target)
                    return true;
                source = VisualTreeHelper.GetParent(source);
            }
            return false;
        }

        private void ColorPicker_Loaded(object sender, RoutedEventArgs e)
        {
            if (PART_ColorImage.Source is BitmapSource source)
            {
                _bitmap = new WriteableBitmap(source);
            }
            SetSelectorFromColor(); // Добавлен вызов позиционирования
        }

        public static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (ColorPicker)d;
            picker.UpdateUIFromColor();

            // Вызываем ивент
            picker.SelectedColorChanged?.Invoke(
                picker,
                new RoutedPropertyChangedEventArgs<Color>(
                    (Color)e.OldValue,
                    (Color)e.NewValue));
        }

        private void UpdateUIFromColor()
        {
            if (!IsLoaded) return;

            PART_R.Text = SelectedColor.R.ToString();
            PART_G.Text = SelectedColor.G.ToString();
            PART_B.Text = SelectedColor.B.ToString();
            PART_ColorPreview.Background = new SolidColorBrush(SelectedColor);

            SetSelectorFromColor(); // Обновляем позицию курсора
        }

        private void ColorImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(PART_ColorImage);
            PickColor(pos);
        }

        public void SetSelectorFromColor()
        {
            if (_bitmap == null || PART_ColorImage == null) return;

            // Преобразование RGB в HSV
            var (hue, saturation, _) = RgbToHsv(SelectedColor.R, SelectedColor.G, SelectedColor.B);

            // Расчет позиции на изображении (предполагается HSV спектр)
            double x = hue / 360.0 * PART_ColorImage.ActualWidth;
            double y = (1 - saturation) * PART_ColorImage.ActualHeight;

            // Обновление позиции селектора
            Canvas.SetLeft(PART_Selector, x - PART_Selector.Width / 2);
            Canvas.SetTop(PART_Selector, y - PART_Selector.Height / 2);
            PART_Selector.Visibility = Visibility.Visible;
        }

        private void PickColor(Point point)
        {
            if (_bitmap == null) return;

            int x = (int)(point.X * _bitmap.PixelWidth / PART_ColorImage.ActualWidth);
            int y = (int)(point.Y * _bitmap.PixelHeight / PART_ColorImage.ActualHeight);

            if (x < 0 || y < 0 || x >= _bitmap.PixelWidth || y >= _bitmap.PixelHeight) return;

            // Буфер для одного пикселя (BGRA)
            byte[] pixel = new byte[4];
            Int32Rect rect = new Int32Rect(x, y, 1, 1);
            _bitmap.CopyPixels(rect, pixel, 4, 0);

            byte b = pixel[0];
            byte g = pixel[1];
            byte r = pixel[2];

            SelectedColor = Color.FromRgb(r, g, b);

            // Позиция кружка
            Canvas.SetLeft(PART_Selector, point.X - PART_Selector.Width / 2);
            Canvas.SetTop(PART_Selector, point.Y - PART_Selector.Height / 2);
            PART_Selector.Visibility = Visibility.Visible;
        }

        private static (double hue, double saturation, double value) RgbToHsv(byte r, byte g, byte b)
        {
            double red = r / 255.0;
            double green = g / 255.0;
            double blue = b / 255.0;

            double max = Math.Max(red, Math.Max(green, blue));
            double min = Math.Min(red, Math.Min(green, blue));
            double delta = max - min;

            double hue = 0;
            if (delta > 0)
            {
                if (max == red) hue = 60 * ((green - blue) / delta % 6);
                else if (max == green) hue = 60 * ((blue - red) / delta + 2);
                else if (max == blue) hue = 60 * ((red - green) / delta + 4);

                if (hue < 0) hue += 360;
            }

            double saturation = max == 0 ? 0 : delta / max;
            return (hue, saturation, max);
        }
        private void Rgb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (byte.TryParse(PART_R.Text, out var r) &&
                byte.TryParse(PART_G.Text, out var g) &&
                byte.TryParse(PART_B.Text, out var b))
            {
                SelectedColor = Color.FromRgb(r, g, b);
            }
        }
    }
}
