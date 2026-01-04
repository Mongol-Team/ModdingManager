using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using UserControl = System.Windows.Controls.UserControl;

namespace ViewControls
{

    public partial class SearchableComboBox : UserControl
    {
        // Expose ItemsSource & SelectedItem so you can bind them in VM
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(SearchableComboBox),
                new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(SearchableComboBox),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ComboWidthProperty =
            DependencyProperty.Register(nameof(ComboWidth), typeof(double), typeof(SearchableComboBox),
                new PropertyMetadata(200d));

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

        private object[] _allItems = Array.Empty<object>();

        public SearchableComboBox()
        {
            InitializeComponent();
            this.PART_ComboBox.Name = this.Name + "ComboBox";
            this.PART_ListBox.Name = this.Name + "ListBox";
            this.PART_Popup.Name = this.Name + "Popup";
            this.PART_SearchBox.Name = this.Name + "SearchBox";
            this.DataContextChanged += (_, __) => RefreshAllItems();
            this.Loaded += (_, __) => RefreshAllItems();
        }

        private void RefreshAllItems()
        {
            if (ItemsSource != null)
                _allItems = ItemsSource.Cast<object>().ToArray();
            if (!string.IsNullOrEmpty(this.Name))
            {
                this.PART_ComboBox.Name = this.Name + "ComboBox";
                this.PART_ListBox.Name = this.Name + "ListBox";
                this.PART_Popup.Name = this.Name + "Popup";
                this.PART_SearchBox.Name = this.Name + "SearchBox";
            }
        }

        private void OnSearchButtonClick(object sender, RoutedEventArgs e)
        {
            PART_SearchBox.Text = "";
            PART_ListBox.ItemsSource = _allItems;
            PART_Popup.IsOpen = true;
            PART_SearchBox.Focus();
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var filter = PART_SearchBox.Text ?? "";
            if (string.IsNullOrEmpty(filter))
                PART_ListBox.ItemsSource = _allItems;
            else
                PART_ListBox.ItemsSource =
                    _allItems
                        .Where(x => x?.ToString()?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToArray();
        }

        private void CommitSelection(object item)
        {
            SelectedItem = item;
            PART_Popup.IsOpen = false;
        }

        // double‑click or Enter picks it
        private void OnListBoxDoubleClick(object sender, MouseButtonEventArgs e)
            => CommitSelection(PART_ListBox.SelectedItem);

        private void OnSearchBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && PART_ListBox.SelectedItem != null)
            {
                CommitSelection(PART_ListBox.SelectedItem);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                PART_Popup.IsOpen = false;
                e.Handled = true;
            }
        }
        public event SelectionChangedEventHandler SelectionChanged
        {
            add => PART_ComboBox.SelectionChanged += value;
            remove => PART_ComboBox.SelectionChanged -= value;
        }

        public event RoutedEventHandler Loaded
        {
            add => PART_ComboBox.Loaded += value;
            remove => PART_ComboBox.Loaded -= value;
        }
    }

}