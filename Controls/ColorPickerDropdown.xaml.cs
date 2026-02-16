using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using UserControl = System.Windows.Controls.UserControl;

namespace Controls
{
    public partial class ColorPickerDropdown : UserControl
    {
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(nameof(SelectedColor), typeof(Color),
                typeof(ColorPickerDropdown),
                new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

        /// <summary>
        /// Событие, вызываемое при изменении цвета пользователем.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<Color>? SelectedColorChanged;
        private bool _isPickerOpen = false;

        public Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public ColorPickerDropdown()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                ColorPreview.Background = new SolidColorBrush(SelectedColor);
                ColorPreview.PreviewMouseLeftButtonUp += ColorPreview_MouseLeftButtonUp;

                ColorPopup.Closed += ColorPopup_Closed;
            };
        }

        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ColorPickerDropdown)d;
            var oldColor = (Color)e.OldValue;
            var newColor = (Color)e.NewValue;

            control.ColorPreview.Background = new SolidColorBrush(newColor);

            // Генерация события при программном или ручном изменении цвета
            control.SelectedColorChanged?.Invoke(
                control,
                new RoutedPropertyChangedEventArgs<Color>(oldColor, newColor)
                {
                    RoutedEvent = SelectedColorChangedEvent
                });
        }

        private void ColorPreview_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isPickerOpen = !_isPickerOpen;

            if (_isPickerOpen)
            {
                // Устанавливаем цвет перед открытием
                Picker.SelectedColor = SelectedColor;
                ColorPopup.IsOpen = true;
                Picker.SetSelectorPos();
                // Принудительно обновляем селектор после рендеринга
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Picker.SetSelectorFromColor();
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
            else
            {
                ColorPopup.IsOpen = false;
            }

            e.Handled = true;
        }
        private void Picker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            SelectedColor = e.NewValue;
            // Убрано закрытие попапа - больше не закрываем при выборе цвета
        }


        private void ColorPopup_Closed(object sender, System.EventArgs e)
        {
            _isPickerOpen = false;
            // Обновляем только предпросмотр
            ColorPreview.Background = new SolidColorBrush(SelectedColor);
        }

        // (необязательно) Зарегистрированный RoutedEvent, если хочешь поддержку XAML RoutedEvent систем
        public static readonly RoutedEvent SelectedColorChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(SelectedColorChanged),
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<Color>),
                typeof(ColorPickerDropdown));
    }
}
