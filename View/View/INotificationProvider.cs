namespace View
{
    public enum NotificationCorner
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public interface INotificationProvider
    {
        void ShowSuccess(string message, NotificationCorner corner = NotificationCorner.TopRight);
        void ShowInfo(string message, NotificationCorner corner = NotificationCorner.TopRight);
        void ShowWarning(string message, NotificationCorner corner = NotificationCorner.TopRight);
        void ShowError(string message, NotificationCorner corner = NotificationCorner.TopRight);
    }
}

