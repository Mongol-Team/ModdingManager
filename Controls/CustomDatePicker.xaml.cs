using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using UserControl = System.Windows.Controls.UserControl;
using WpfCalendarControl = System.Windows.Controls.Calendar;
using GlobalizationCalendar = System.Globalization.Calendar;

namespace Controls
{
    public partial class CustomDatePicker : UserControl
    {
        private global::System.Windows.Controls.Calendar? _calendar;
        private global::System.Windows.Controls.TextBox? _dateTextBox;
        private Popup? _calendarPopup;

        public static readonly DependencyProperty SelectedDateProperty =
            DependencyProperty.Register(nameof(SelectedDate), typeof(DateTime?), typeof(CustomDatePicker),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateChanged));

        public static readonly DependencyProperty TagProperty =
            DependencyProperty.Register(nameof(Tag), typeof(string), typeof(CustomDatePicker),
                new PropertyMetadata(string.Empty));

        public static readonly RoutedEvent SelectedDateChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(SelectedDateChanged), RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<DateTime?>), typeof(CustomDatePicker));

        public event RoutedPropertyChangedEventHandler<DateTime?> SelectedDateChanged
        {
            add => AddHandler(SelectedDateChangedEvent, value);
            remove => RemoveHandler(SelectedDateChangedEvent, value);
        }

        public DateTime? SelectedDate
        {
            get => (DateTime?)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        public new string Tag
        {
            get => (string)GetValue(TagProperty);
            set => SetValue(TagProperty, value);
        }

        public CustomDatePicker()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var calendarObj = FindName("DateCalendar");
            _calendar = calendarObj as global::System.Windows.Controls.Calendar;
            _dateTextBox = FindName("DateTextBox") as System.Windows.Controls.TextBox;
            _calendarPopup = FindName("CalendarPopup") as Popup;

            if (_calendar != null)
            {
                var cal = (global::System.Windows.Controls.Calendar)_calendar;
                cal.SelectedDatesChanged += Calendar_SelectedDateChanged;
                cal.DisplayDateChanged += Calendar_DisplayDateChanged;
            }
            UpdateTextBox();
        }

        private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CustomDatePicker picker)
            {
                picker.UpdateTextBox();
                var oldValue = (DateTime?)e.OldValue;
                var newValue = (DateTime?)e.NewValue;
                var args = new RoutedPropertyChangedEventArgs<DateTime?>(oldValue, newValue)
                {
                    RoutedEvent = SelectedDateChangedEvent
                };
                picker.RaiseEvent(args);
            }
        }

        private void UpdateTextBox()
        {
            if (_dateTextBox != null)
            {
                if (SelectedDate.HasValue)
                {
                    _dateTextBox.Text = SelectedDate.Value.ToString("dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    _dateTextBox.Text = string.Empty;
                }
            }
        }

        private void CalendarButton_Click(object sender, RoutedEventArgs e)
        {
            if (_calendarPopup != null)
            {
                _calendarPopup.IsOpen = !_calendarPopup.IsOpen;
            }
        }

        private void Calendar_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_calendar != null && _calendar.SelectedDate.HasValue)
            {
                SelectedDate = _calendar.SelectedDate.Value;
                if (_calendarPopup != null)
                {
                    _calendarPopup.IsOpen = false;
                }
            }
        }

        private void CalendarPopup_Opened(object sender, EventArgs e)
        {
            if (_calendar != null)
            {
                if (SelectedDate.HasValue)
                {
                    _calendar.SelectedDate = SelectedDate.Value;
                    _calendar.DisplayDate = SelectedDate.Value;
                }
                else
                {
                    _calendar.SelectedDate = null;
                    _calendar.DisplayDate = DateTime.Today;
                }
            }
        }

        private void CalendarPopup_Closed(object sender, EventArgs e)
        {
            // Календарь закрыт
        }

        private void Calendar_DisplayDateChanged(object sender, EventArgs e)
        {
            // Обновление отображаемой даты
        }
    }
}

