using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UserControl = System.Windows.Controls.UserControl;

namespace ModdingManager.Controls
{
    public partial class ImageButton : UserControl
    {
        public ImageButton()
        {
            InitializeComponent();
            this.PreviewMouseDown += ImageButton_PreviewMouseDown;
        }

        #region Dependency Properties

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(ImageButton));

        private void ImageButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Создаем новое событие MouseDown и поднимаем его на текущем элементе
            var args = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton)
            {
                RoutedEvent = UIElement.MouseDownEvent,
                Source = this,
            };

            RaiseEvent(args);

            // Дополнительно, если нужно выполнить команду:
            if (Command?.CanExecute(CommandParameter) == true)
            {
                Command.Execute(CommandParameter);
            }

        }

        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(ImageButton));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(ImageButton));

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        #endregion

        #region Events

        // Не нужно создавать отдельное RoutedEvent для MouseDown - используем стандартное
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            // Теперь событие будет всплывать нормально
        }

        #endregion

        #region Button Handlers

        private void InternalButton_Click(object sender, RoutedEventArgs e)
        {
            if (Command?.CanExecute(CommandParameter) == true)
            {
                Command.Execute(CommandParameter);
            }
            e.Handled = true; // Останавливаем дальнейшую обработку
        }

        #endregion
    }
}