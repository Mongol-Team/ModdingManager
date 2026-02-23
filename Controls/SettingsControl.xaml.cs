using System.Collections;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace Controls
{
    public partial class SettingsControl : UserControl
    {
        public static readonly DependencyProperty GameDirectoryProperty =
            DependencyProperty.Register(nameof(GameDirectory), typeof(string), typeof(SettingsControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnGameDirectoryChanged));

        public static readonly DependencyProperty ParallelismPercentProperty =
            DependencyProperty.Register(nameof(ParallelismPercent), typeof(int), typeof(SettingsControl),
                new FrameworkPropertyMetadata(50, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnParallelismPercentChanged));

        public static readonly DependencyProperty IsDebugModeProperty =
            DependencyProperty.Register(nameof(IsDebugMode), typeof(bool), typeof(SettingsControl),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsDebugModeChanged));

        public static readonly DependencyProperty LanguageItemsSourceProperty =
            DependencyProperty.Register(nameof(LanguageItemsSource), typeof(IEnumerable), typeof(SettingsControl),
                new PropertyMetadata(null, OnLanguageItemsSourceChanged));

        public static readonly DependencyProperty SelectedLanguageProperty =
            DependencyProperty.Register(nameof(SelectedLanguage), typeof(object), typeof(SettingsControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedLanguageChanged));

        public static readonly DependencyProperty EffectiveLanguageProperty =
            DependencyProperty.Register(nameof(EffectiveLanguage), typeof(object), typeof(SettingsControl),
                new PropertyMetadata(null, OnEffectiveLanguageChanged));

        public static readonly DependencyProperty ClassDebugNamesProperty =
            DependencyProperty.Register(nameof(ClassDebugNames), typeof(List<string>), typeof(SettingsControl),
                new FrameworkPropertyMetadata(new List<string>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly RoutedEvent SaveClickedEvent =
            EventManager.RegisterRoutedEvent(nameof(SaveClicked), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SettingsControl));

        public event RoutedEventHandler SaveClicked
        {
            add => AddHandler(SaveClickedEvent, value);
            remove => RemoveHandler(SaveClickedEvent, value);
        }

        public int ParallelismPercent
        {
            get => (int)GetValue(ParallelismPercentProperty);
            set => SetValue(ParallelismPercentProperty, value);
        }

        public bool IsDebugMode
        {
            get => (bool)GetValue(IsDebugModeProperty);
            set => SetValue(IsDebugModeProperty, value);
        }

        public IEnumerable LanguageItemsSource
        {
            get => (IEnumerable)GetValue(LanguageItemsSourceProperty);
            set => SetValue(LanguageItemsSourceProperty, value);
        }

        public object SelectedLanguage
        {
            get => GetValue(SelectedLanguageProperty);
            set => SetValue(SelectedLanguageProperty, value);
        }

        public object EffectiveLanguage
        {
            get => GetValue(EffectiveLanguageProperty);
            set => SetValue(EffectiveLanguageProperty, value);
        }

        public string GameDirectory
        {
            get => (string)GetValue(GameDirectoryProperty);
            set => SetValue(GameDirectoryProperty, value);
        }
        public List<string> ClassDebugNames
        {
            get => (List<string>)GetValue(ClassDebugNamesProperty);
            set => SetValue(ClassDebugNamesProperty, value);
        }
        public SettingsControl()
        {
            InitializeComponent();
        }

        private static void OnParallelismPercentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SettingsControl c && c.ParallelismValueText != null)
                c.ParallelismValueText.Text = $"{(int)e.NewValue}%";
            if (d is SettingsControl c2 && c2.ParallelismSlider != null && (int)e.NewValue != (int)c2.ParallelismSlider.Value)
                c2.ParallelismSlider.Value = (int)e.NewValue;
        }

        private static void OnIsDebugModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SettingsControl c && c.DebugModeCheckBox != null)
                c.DebugModeCheckBox.IsChecked = (bool)e.NewValue;
        }

        private static void OnLanguageItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SettingsControl c && c.LanguageComboBox != null)
                c.LanguageComboBox.ItemsSource = (IEnumerable)e.NewValue;
        }

        private static void OnSelectedLanguageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SettingsControl c && c.LanguageComboBox != null)
                c.LanguageComboBox.SelectedItem = e.NewValue;
            if (d is SettingsControl c2)
                c2.UpdateLanguageRestartHintVisibility();
        }

        private static void OnEffectiveLanguageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SettingsControl c)
                c.UpdateLanguageRestartHintVisibility();
        }

        private static void OnGameDirectoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SettingsControl c && c.GameDirectoryTextBox != null)
                c.GameDirectoryTextBox.Text = (string)e.NewValue ?? string.Empty;
        }

        private void BrowseGameDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
                GameDirectory = dialog.FolderName;
        }

        private void UpdateLanguageRestartHintVisibility()
        {
            if (LanguageRestartHintBlock == null) return;
            var selected = SelectedLanguage;
            var effective = EffectiveLanguage;
            LanguageRestartHintBlock.Visibility = selected != null && effective != null && !Equals(selected, effective)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void ParallelismSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ParallelismValueText != null)
                ParallelismValueText.Text = $"{(int)e.NewValue}%";
            ParallelismPercent = (int)e.NewValue;
        }

        private void DebugModeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            IsDebugMode = true;
        }

        private void DebugModeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            IsDebugMode = false;
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox?.SelectedItem != null)
            {
                SelectedLanguage = LanguageComboBox.SelectedItem;
                UpdateLanguageRestartHintVisibility();
            }
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SaveClickedEvent));
        }
    }
}
