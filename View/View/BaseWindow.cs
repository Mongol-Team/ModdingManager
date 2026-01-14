using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ViewControls;
using NotificationType = ViewControls.NotificationType;
using Orientation = System.Windows.Controls.Orientation;

namespace View
{
    public abstract class BaseWindow : Window, INotificationProvider
    {
        private readonly Dictionary<NotificationCorner, System.Windows.Controls.Panel> _notificationContainers = new();
        private const int MaxNotifications = 5;
        private const int AutoDismissSeconds = 5;

        protected BaseWindow()
        {
            InitializeNotificationContainers();
        }

        private void InitializeNotificationContainers()
        {
            Loaded += (s, e) =>
            {
                if (Content is FrameworkElement rootElement)
                {
                    var overlayGrid = new Grid
                    {
                        IsHitTestVisible = false,
                        Margin = new Thickness(16)
                    };

                    foreach (NotificationCorner corner in Enum.GetValues(typeof(NotificationCorner)))
                    {
                        var container = CreateNotificationContainer(corner);
                        overlayGrid.Children.Add(container);
                        _notificationContainers[corner] = container;
                    }

                    System.Windows.Controls.Panel parentPanel;
                    if (rootElement is System.Windows.Controls.Panel existingPanel)
                    {
                        parentPanel = existingPanel;
                    }
                    else
                    {
                        var wrapper = new Grid();
                        var oldContent = Content;
                        Content = null;
                        Content = wrapper;
                        if (oldContent is UIElement oldElement)
                        {
                            wrapper.Children.Add(oldElement);
                        }
                        parentPanel = wrapper;
                    }

                    parentPanel.Children.Add(overlayGrid);
                }
            };
        }

        private System.Windows.Controls.Panel CreateNotificationContainer(NotificationCorner corner)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            switch (corner)
            {
                case NotificationCorner.TopLeft:
                    stackPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    stackPanel.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    break;
                case NotificationCorner.TopRight:
                    stackPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    stackPanel.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    break;
                case NotificationCorner.BottomLeft:
                    stackPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    stackPanel.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                    break;
                case NotificationCorner.BottomRight:
                    stackPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    stackPanel.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                    break;
            }

            return stackPanel;
        }

        public void ShowSuccess(string message, NotificationCorner corner = NotificationCorner.TopRight)
        {
            ShowNotification(message, NotificationType.Success, corner);
        }

        public void ShowInfo(string message, NotificationCorner corner = NotificationCorner.TopRight)
        {
            ShowNotification(message, NotificationType.Info, corner);
        }

        public void ShowWarning(string message, NotificationCorner corner = NotificationCorner.TopRight)
        {
            ShowNotification(message, NotificationType.Warning, corner);
        }

        public void ShowError(string message, NotificationCorner corner = NotificationCorner.TopRight)
        {
            ShowNotification(message, NotificationType.Error, corner);
        }

        private void ShowNotification(string message, NotificationType type, NotificationCorner corner)
        {
            Dispatcher.Invoke(() =>
            {
                if (!_notificationContainers.TryGetValue(corner, out var container))
                {
                    return;
                }

                var notification = new NotificationControl(message, type);

                if (container is StackPanel stackPanel)
                {
                    if (stackPanel.Children.Count >= MaxNotifications)
                    {
                        var oldest = stackPanel.Children[0];
                        stackPanel.Children.RemoveAt(0);
                        if (oldest is NotificationControl oldNotification)
                        {
                            oldNotification.Dismiss();
                        }
                    }

                    stackPanel.Children.Insert(0, notification);
                    notification.Margin = new Thickness(0, 0, 0, 8);
                }

                notification.Show();

                var timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(AutoDismissSeconds)
                };
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    notification.Dismiss();
                };
                timer.Start();
            });
        }
    }
}

