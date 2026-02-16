using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using UserControl = System.Windows.Controls.UserControl;
using Brush = System.Windows.Media.Brush;
using Panel = System.Windows.Controls.Panel;

namespace Controls
{
    public partial class NotificationControl : UserControl
    {
        public static readonly DependencyProperty IconColorProperty =
            DependencyProperty.Register(nameof(IconColor), typeof(Brush), typeof(NotificationControl),
                new PropertyMetadata(new SolidColorBrush(Colors.White)));

        public Brush IconColor
        {
            get => (Brush)GetValue(IconColorProperty);
            set => SetValue(IconColorProperty, value);
        }

        private readonly NotificationType _type;

        public NotificationControl(string message, NotificationType type)
        {
            InitializeComponent();
            _type = type;
            MessageTextBlock.Text = message;
            SetupNotificationType();
        }

        private void SetupNotificationType()
        {
            switch (_type)
            {
                case NotificationType.Success:
                    IconColor = (Brush)TryFindResource("Success") ?? new SolidColorBrush(Colors.Green);
                    IconPath.Data = Geometry.Parse("M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z");
                    NotificationBorder.BorderBrush = (Brush)TryFindResource("Success") ?? new SolidColorBrush(Colors.Green);
                    break;
                case NotificationType.Info:
                    IconColor = (Brush)TryFindResource("Link") ?? new SolidColorBrush(Colors.Blue);
                    IconPath.Data = Geometry.Parse("M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z");
                    NotificationBorder.BorderBrush = (Brush)TryFindResource("Link") ?? new SolidColorBrush(Colors.Blue);
                    break;
                case NotificationType.Warning:
                    IconColor = (Brush)TryFindResource("Warning") ?? new SolidColorBrush(Colors.Orange);
                    IconPath.Data = Geometry.Parse("M1 21h22L12 2 1 21zm12-3h-2v-2h2v2zm0-4h-2v-4h2v4z");
                    NotificationBorder.BorderBrush = (Brush)TryFindResource("Warning") ?? new SolidColorBrush(Colors.Orange);
                    break;
                case NotificationType.Error:
                    IconColor = (Brush)TryFindResource("Error") ?? new SolidColorBrush(Colors.Red);
                    IconPath.Data = Geometry.Parse("M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z");
                    NotificationBorder.BorderBrush = (Brush)TryFindResource("Error") ?? new SolidColorBrush(Colors.Red);
                    break;
            }
        }

        public void Show()
        {
            var fadeIn = (Storyboard)Resources["FadeInAnimation"];
            fadeIn.Begin();
        }

        public void Dismiss()
        {
            var fadeOut = (Storyboard)Resources["FadeOutAnimation"];
            fadeOut.Completed += (s, e) =>
            {
                if (Parent is Panel parent)
                {
                    parent.Children.Remove(this);
                }
            };
            fadeOut.Begin();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Dismiss();
        }
    }

    public enum NotificationType
    {
        Success,
        Info,
        Warning,
        Error
    }
}

