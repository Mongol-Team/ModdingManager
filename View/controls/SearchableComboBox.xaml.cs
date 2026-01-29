using Application.Extentions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using UserControl = System.Windows.Controls.UserControl;

namespace ViewControls
{
    public partial class SearchableComboBox : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
           DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(SearchableComboBox),
               new PropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(SearchableComboBox),
                new PropertyMetadata(null, OnSelectedItemChanged));

        public static readonly DependencyProperty ComboWidthProperty =
            DependencyProperty.Register(nameof(ComboWidth), typeof(double), typeof(SearchableComboBox),
                new PropertyMetadata(200d, OnComboWidthChanged));

        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(SearchableComboBox),
                new PropertyMetadata(false, OnIsDropDownOpenChanged));

        // Events
        public event SelectionChangedEventHandler SelectionChanged;
        public event EventHandler DropDownOpened;
        public event EventHandler DropDownClosed;
        private int _startIndex = 0;
        private const int _windowSize = 100;
        private List<object> _filteredItems = new List<object>();

        // Fields
        private List<object> _allItems = new List<object>();
        private bool _isUpdatingSelection = false;
        private bool _isUpdatingFromProperty = false;

        public SearchableComboBox()
        {
            InitializeComponent();
            InitializeEvents();
            PART_ListBox.Loaded += (s, e) => { var sv = PART_ListBox.FindVisualChildren<ScrollViewer>().FirstOrDefault(); if (sv != null) { sv.ScrollChanged += OnScrollChanged; } };
        }

        private void InitializeEvents()
        {
            // ВАЖНО: Убрали MouseLeftButtonDown с PART_ComboBoxContainer
            // Теперь только кнопка управляет открытием/закрытием

            PART_Popup.Closed += (s, e) =>
            {
                if (!_isUpdatingFromProperty)
                {
                    _isUpdatingFromProperty = true;
                    IsDropDownOpen = false;
                    _isUpdatingFromProperty = false;
                }
                DropDownClosed?.Invoke(this, e);
                PART_SearchBox.Clear();
                UpdateListBoxItems();
            };

            PART_Popup.Opened += (s, e) =>
            {
                if (!_isUpdatingFromProperty)
                {
                    _isUpdatingFromProperty = true;
                    IsDropDownOpen = true;
                    _isUpdatingFromProperty = false;
                }
                DropDownOpened?.Invoke(this, e);
                PART_SearchBox.Focus();
                HighlightSelectedItemInList();
            };
         
        }
        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var sv = sender as ScrollViewer;
            if (sv == null) return;

            // вниз
            if (sv.VerticalOffset >= sv.ScrollableHeight)
            {
                if (_startIndex + _windowSize < _filteredItems.Count)
                {
                    _startIndex++;
                    PART_ListBox.ItemsSource = _filteredItems.Skip(_startIndex).Take(_windowSize).ToList();
                    sv.ScrollToVerticalOffset(sv.VerticalOffset - 1);
                }
            }
            // вверх
            else if (sv.VerticalOffset <= 0)
            {
                if (_startIndex > 0)
                {
                    _startIndex--;
                    PART_ListBox.ItemsSource = _filteredItems.Skip(_startIndex).Take(_windowSize).ToList();
                    sv.ScrollToVerticalOffset(sv.VerticalOffset + 1);
                }
            }
        }

        // Properties
        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public double ComboWidth
        {
            get => (double)GetValue(ComboWidthProperty);
            set => SetValue(ComboWidthProperty, value);
        }

        public bool IsDropDownOpen
        {
            get => (bool)GetValue(IsDropDownOpenProperty);
            set => SetValue(IsDropDownOpenProperty, value);
        }

        // Event Handlers
        private void OnDropDownButtonClick(object sender, RoutedEventArgs e)
        {
            // Простая логика - переключаем состояние
            IsDropDownOpen = !IsDropDownOpen;
            e.Handled = true;
        }

        private void OnClearSearchClick(object sender, RoutedEventArgs e)
        {
            PART_SearchBox.Clear();
            PART_SearchBox.Focus();
            e.Handled = true;
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateListBoxItems();

            // Показывать/скрывать кнопку очистки
            PART_ClearButton.Visibility = string.IsNullOrEmpty(PART_SearchBox.Text)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void UpdateListBoxItems()
        {
            var searchText = PART_SearchBox.Text ?? "";

            if (string.IsNullOrEmpty(searchText))
            {
                _filteredItems = _allItems;
            }
            else
            {
                _filteredItems = _allItems
                    .Where(item => item?.ToString()?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
                _startIndex = 0;
            }

            PART_ListBox.ItemsSource = _filteredItems.Skip(_startIndex).Take(_windowSize).ToList();
        }


        private void HighlightSelectedItemInList()
        {
            if (SelectedItem != null && PART_ListBox.Items.Contains(SelectedItem))
            {
                PART_ListBox.SelectedItem = SelectedItem;
                PART_ListBox.ScrollIntoView(SelectedItem);
            }
            else
            {
                PART_ListBox.SelectedItem = null;
            }
        }

        private void OnListBoxDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PART_ListBox.SelectedItem != null)
            {
                CommitSelection(PART_ListBox.SelectedItem);
                e.Handled = true;
            }
        }

        private void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isUpdatingSelection && PART_ListBox.SelectedItem != null)
            {
                CommitSelection(PART_ListBox.SelectedItem);
            }
        }

        private void OnComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Синхронизация с реальным ComboBox
            if (!_isUpdatingSelection)
            {
                _isUpdatingSelection = true;
                SelectedItem = PART_ComboBox.SelectedItem;
                _isUpdatingSelection = false;
            }
        }

        private void OnSearchBoxKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (PART_ListBox.SelectedItem != null)
                    {
                        CommitSelection(PART_ListBox.SelectedItem);
                    }
                    else if (PART_ListBox.Items.Count > 0)
                    {
                        PART_ListBox.SelectedIndex = 0;
                        CommitSelection(PART_ListBox.SelectedItem);
                    }
                    e.Handled = true;
                    break;

                case Key.Escape:
                    IsDropDownOpen = false;
                    e.Handled = true;
                    break;

                case Key.Down:
                    if (PART_ListBox.Items.Count > 0)
                    {
                        PART_ListBox.Focus();
                        if (PART_ListBox.SelectedIndex < PART_ListBox.Items.Count - 1)
                            PART_ListBox.SelectedIndex++;
                        else
                            PART_ListBox.SelectedIndex = 0;
                    }
                    e.Handled = true;
                    break;

                case Key.Up:
                    if (PART_ListBox.Items.Count > 0)
                    {
                        PART_ListBox.Focus();
                        if (PART_ListBox.SelectedIndex > 0)
                            PART_ListBox.SelectedIndex--;
                        else
                            PART_ListBox.SelectedIndex = PART_ListBox.Items.Count - 1;
                    }
                    e.Handled = true;
                    break;
            }
        }

        private void CommitSelection(object item)
        {
            _isUpdatingSelection = true;
            SelectedItem = item;
            PART_ComboBox.SelectedItem = item;
            IsDropDownOpen = false;

            // Вызываем событие SelectionChanged
            var args = new SelectionChangedEventArgs(Selector.SelectionChangedEvent,
                new List<object>(),
                new List<object> { item });
            SelectionChanged?.Invoke(this, args);

            _isUpdatingSelection = false;
        }

        // Dependency Property Changed Handlers
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SearchableComboBox control)
            {
                control.RefreshAllItems();
                control.UpdateListBoxItems();
            }
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SearchableComboBox control && !control._isUpdatingSelection)
            {
                control._isUpdatingSelection = true;
                control.PART_ComboBox.SelectedItem = e.NewValue;
                control._isUpdatingSelection = false;
            }
        }

        private static void OnComboWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SearchableComboBox control)
            {
                control.PART_ComboBoxContainer.Width = (double)e.NewValue;
                control.PART_Popup.Width = (double)e.NewValue;
            }
        }

        private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SearchableComboBox control && !control._isUpdatingFromProperty)
            {
                var newValue = (bool)e.NewValue;

                if (newValue)
                {
                    control.OpenDropDown();
                }
                else
                {
                    control.CloseDropDown();
                }
            }
        }

        private void RefreshAllItems()
        {
            if (ItemsSource != null)
            {
                _allItems = ItemsSource.Cast<object>().ToList();
            }
            else
            {
                _allItems = new List<object>();
            }
        }

        // Public Methods
        public void ClearSelection()
        {
            SelectedItem = null;
            PART_ComboBox.SelectedItem = null;
        }

        public void OpenDropDown()
        {
            if (!PART_Popup.IsOpen)
            {
                UpdateListBoxItems();
                HighlightSelectedItemInList();
                PART_Popup.IsOpen = true;
                UpdateArrowRotation(true);
            }
        }

        public void CloseDropDown()
        {
            if (PART_Popup.IsOpen)
            {
                PART_Popup.IsOpen = false;
                UpdateArrowRotation(false);
            }
        }

        private void UpdateArrowRotation(bool isOpen)
        {
            // Вращаем стрелку
            if (PART_ArrowIcon.RenderTransform is RotateTransform rotateTransform)
            {
                rotateTransform.Angle = isOpen ? 180 : 0;
            }
            else
            {
                PART_ArrowIcon.RenderTransform = new RotateTransform(isOpen ? 180 : 0, 4, 4);
            }
        }
    }

    // Конвертер для отображения объектов как строк
    public class ObjectToStringConverter : IValueConverter
    {
        public static readonly ObjectToStringConverter Default = new ObjectToStringConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}