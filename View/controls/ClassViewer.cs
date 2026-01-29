using Application;
using Application.Debugging;
using Application.Extentions;
using Data;
using ModdingManager.classes.utils;
using Models.Args;
using Models.Configs;
using Models.Enums;
using Models.GfxTypes;
using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.ObjectCacheData;
using Models.Types.Utils;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using View.Utils;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using Color = System.Windows.Media.Color;
using ComboBox = System.Windows.Controls.ComboBox;
using DataFormats = System.Windows.DataFormats;
using Image = System.Windows.Controls.Image;
using Label = System.Windows.Controls.Label;
using Orientation = System.Windows.Controls.Orientation;
using Panel = System.Windows.Controls.Panel;
using Size = System.Windows.Size;
using TextBox = System.Windows.Controls.TextBox;

namespace ViewControls
{
    public class ClassViewer : StackPanel
    {
        public enum ContentOrientation
        {
            Left,
            Center,
            Right
        }

        private object _buildingContent;
        private bool _isHeightExplicitlySet = false;
        private bool _isWidthExplicitlySet = false;
        private ContentOrientation _elementOrientation = ContentOrientation.Left;

        public event EventHandler<PropertyChangedEventArg> OnPropertyChange;

        public object BuildingContent
        {
            get => _buildingContent;
            set
            {
                _buildingContent = value;
                BuildUI();
            }
        }

        public double FontSize { get; set; } = 12;
        public Thickness ElementMargin { get; set; } = new Thickness(5);

        public ContentOrientation ElementOrientation
        {
            get => _elementOrientation;
            set
            {
                if (_elementOrientation != value)
                {
                    _elementOrientation = value;
                    UpdateElementsAlignment();
                }
            }
        }

        public ClassViewer()
        {
            Orientation = System.Windows.Controls.Orientation.Vertical;

            Loaded += (s, e) =>
            {
                _isHeightExplicitlySet = !double.IsNaN(Height);
                _isWidthExplicitlySet = !double.IsNaN(Width);
            };
        }

        private void UpdateElementsAlignment()
        {
            foreach (UIElement child in Children)
            {
                if (child is FrameworkElement fe)
                {
                    switch (_elementOrientation)
                    {
                        case ContentOrientation.Left:
                            fe.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                            break;
                        case ContentOrientation.Center:
                            fe.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                            break;
                        case ContentOrientation.Right:
                            fe.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                            break;
                    }
                }
            }
        }



        private void RaisePropertyChanged(PropertyInfo prop, object oldValue, object newValue)
        {
            if (!Equals(oldValue, newValue))
                OnPropertyChange?.Invoke(this, new PropertyChangedEventArg(prop.Name, oldValue, newValue));
        }
        private void BuildUI()
        {
            Children.Clear();
            if (_buildingContent == null) return;

            var props = _buildingContent.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite &&
                            p.GetCustomAttribute<JsonIgnoreAttribute>() == null);

            foreach (var prop in props)
            {
                FrameworkElement inputControl = null;
                var value = prop.GetValue(_buildingContent);

                if (prop.PropertyType == typeof(string))
                {
                    var textBox = new TextBox
                    {
                        Text = value?.ToString(),
                        Margin = ElementMargin,
                        Width = this.Width,
                        Style = (Style)TryFindResource("GenericViewerTextBox"),
                        Tag = prop.Name
                    };
                    textBox.TextChanged += (s, e) =>
                    {
                        var old = prop.GetValue(_buildingContent);
                        var newText = textBox.Text;
                        prop.SetValue(_buildingContent, newText);
                        RaisePropertyChanged(prop, old, newText);
                    };
                    inputControl = textBox;
                }
                else if (prop.PropertyType == typeof(Identifier))
                {
                    var identifier = value as Identifier;
                    var textBox = new TextBox
                    {
                        Text = identifier?.ToString() ?? string.Empty,
                        Margin = ElementMargin,
                        Width = this.Width,
                        Style = (Style)TryFindResource("GenericViewerTextBox"),
                        Tag = prop.Name
                    };
                    textBox.TextChanged += (s, e) =>
                    {
                        var old = prop.GetValue(_buildingContent);
                        var newIdentifier = new Identifier(textBox.Text);
                        prop.SetValue(_buildingContent, newIdentifier);
                        RaisePropertyChanged(prop, old, newIdentifier);
                    };
                    inputControl = textBox;
                }
                else if (GetNullableUnderlyingType(prop.PropertyType) == typeof(Identifier))
                {
                    var identifier = value as Identifier;
                    var textBox = new TextBox
                    {
                        Text = identifier?.ToString() ?? string.Empty,
                        Margin = ElementMargin,
                        Width = this.Width,
                        Style = (Style)TryFindResource("GenericViewerTextBox"),
                        Tag = prop.Name
                    };
                    textBox.TextChanged += (s, e) =>
                    {
                        var old = prop.GetValue(_buildingContent);
                        Identifier newIdentifier = null;
                        if (!string.IsNullOrWhiteSpace(textBox.Text))
                        {
                            newIdentifier = new Identifier(textBox.Text);
                        }
                        prop.SetValue(_buildingContent, newIdentifier);
                        RaisePropertyChanged(prop, old, newIdentifier);
                    };
                    inputControl = textBox;
                }
                else if (prop.PropertyType.IsEnum || (prop.PropertyType.IsGenericType && 
                         prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && 
                         prop.PropertyType.GetGenericArguments()[0].IsEnum))
                {
                    Type enumType = prop.PropertyType.IsEnum ? prop.PropertyType : prop.PropertyType.GetGenericArguments()[0];
                    var enumValues = Enum.GetValues(enumType);
                    var comboBox = new SearchableComboBox
                    {
                        ItemsSource = enumValues,
                        SelectedItem = value,
                        Margin = ElementMargin,
                        Width = this.Width,
                        Style = (Style)TryFindResource("GenericViewerSearchableComboBox")
                    };
                    comboBox.SelectionChanged += (s, e) =>
                    {
                        var old = prop.GetValue(_buildingContent);
                        prop.SetValue(_buildingContent, comboBox.SelectedItem);
                        RaisePropertyChanged(prop, old, comboBox.SelectedItem);
                    };
                    inputControl = comboBox;
                }
                else if (prop.PropertyType == typeof(ConfigLocalisation))
                {
                    var localisation = value as ConfigLocalisation;
                    if (localisation == null)
                    {
                        localisation = new ConfigLocalisation();
                        prop.SetValue(_buildingContent, localisation);
                    }
                    
                    var localisationDisplay = new LocalisationDisplay
                    {
                        Localisation = localisation,
                        Margin = ElementMargin,
                        Width = this.Width,
                    };
                    
                    var languageComboBox = new SearchableComboBox
                    {
                        ItemsSource = Enum.GetValues(typeof(Models.Enums.Language)).Cast<Models.Enums.Language>(),
                        SelectedItem = localisation.Language,
                        Margin = ElementMargin,
                        Width = this.Width,
                        Style = (Style)TryFindResource("GenericViewerSearchableComboBox")
                    };
                    languageComboBox.SelectionChanged += (s, e) =>
                    {
                        if (languageComboBox.SelectedItem is Models.Enums.Language selectedLanguage)
                        {
                            var old = localisation.Language;
                            localisation.Language = selectedLanguage;
                            RaisePropertyChanged(prop, old, selectedLanguage);
                        }
                    };
                    
                    var stackPanel = new StackPanel
                    {
                        Orientation = Orientation.Vertical
                    };
                    
                    var languageRow = new Grid
                    {
                        Margin = new Thickness(0, ElementMargin.Top, 0, ElementMargin.Bottom)
                    };
                    languageRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                    languageRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    languageRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                    
                    var languageLabel = new TextBlock
                    {
                        Text = "Language",
                        Style = (Style)TryFindResource("GenericViewerSectionTitle"),
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        Margin = new Thickness(ElementMargin.Left, 0, 10, 0)
                    };
                    
                    if (languageComboBox is FrameworkElement fe1)
                    {
                        fe1.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                        fe1.Margin = new Thickness(0, 0, ElementMargin.Right, 0);
                    }
                    
                    Grid.SetColumn(languageLabel, 0);
                    Grid.SetColumn(languageComboBox, 2);
                    languageRow.Children.Add(languageLabel);
                    languageRow.Children.Add(languageComboBox);
                    if (this.Width > 0)
                    {
                        languageRow.Width = this.Width;
                    }
                    stackPanel.Children.Add(languageRow);
                    
                    var localisationRow = new Grid
                    {
                        Margin = new Thickness(0, ElementMargin.Top + 10, 0, ElementMargin.Bottom)
                    };
                    localisationRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                    localisationRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    localisationRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                    
                    var localisationLabel = new TextBlock
                    {
                        Text = "Localisation",
                        Style = (Style)TryFindResource("GenericViewerSectionTitle"),
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        Margin = new Thickness(ElementMargin.Left, 0, 10, 0)
                    };
                    
                    if (localisationDisplay is FrameworkElement fe2)
                    {
                        fe2.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                        fe2.Margin = new Thickness(0, 0, ElementMargin.Right, 0);
                    }
                    
                    Grid.SetColumn(localisationLabel, 0);
                    Grid.SetColumn(localisationDisplay, 2);
                    localisationRow.Children.Add(localisationLabel);
                    localisationRow.Children.Add(localisationDisplay);
                    if (this.Width > 0)
                    {
                        localisationRow.Width = this.Width;
                    }
                    stackPanel.Children.Add(localisationRow);
                    
                    inputControl = stackPanel;
                }
                else if (prop.PropertyType == typeof(List<Var>))
                {
                    var list = (List<Var>?)prop.GetValue(_buildingContent) ?? new List<Var>();
                    var sb = new StringBuilder();
                    foreach (var item in list)
                    {
                        if (item == null || string.IsNullOrWhiteSpace(item.Name)) continue;

                        var valStr = item.Value?.ToString();
                        if (valStr != null)
                            valStr = valStr.Replace("\r", "").Replace("\n", " ");
                        sb.AppendLine($"{item.Name} = {valStr}");
                    }
                    var textBox = new TextBox
                    {
                        Text = sb.ToString(),
                        AcceptsReturn = true,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        Margin = ElementMargin,
                        Height = 100,
                        TextWrapping = TextWrapping.Wrap,
                        Width = this.Width,
                        Style = (Style)TryFindResource("GenericViewerTextBox"),
                        Tag = prop.Name
                    };

                    textBox.TextChanged += (s, e) =>
                    {
                        var newList = new List<Var>();
                        var lines = textBox.Text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var line in lines)
                        {
                            var trimmedLine = line.Trim();
                            if (string.IsNullOrWhiteSpace(trimmedLine)) continue;

                            var eqIndex = trimmedLine.IndexOf('=');
                            if (eqIndex <= 0) continue;

                            var namePart = trimmedLine.Substring(0, eqIndex).Trim();
                            var valuePart = eqIndex < trimmedLine.Length - 1
                                ? trimmedLine.Substring(eqIndex + 1).Trim()
                                : null;

                            if (string.IsNullOrWhiteSpace(namePart)) continue;

                            newList.Add(new Var
                            {
                                Name = namePart,
                                Value = !string.IsNullOrWhiteSpace(valuePart) ? valuePart : null
                            });
                        }

                        var oldList = prop.GetValue(_buildingContent);
                        prop.SetValue(_buildingContent, newList);
                        RaisePropertyChanged(prop, oldList, newList);
                    };

                    inputControl = textBox;
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    var checkBox = new CheckBox
                    {
                        IsChecked = (bool?)value,
                        Content = prop.Name,
                        Margin = ElementMargin,
                        Width = this.Width,
                    };
                    checkBox.Checked += (s, e) =>
                    {
                        var old = prop.GetValue(_buildingContent);
                        prop.SetValue(_buildingContent, true);
                        RaisePropertyChanged(prop, old, true);
                    };
                    checkBox.Unchecked += (s, e) =>
                    {
                        var old = prop.GetValue(_buildingContent);
                        prop.SetValue(_buildingContent, false);
                        RaisePropertyChanged(prop, old, false);
                    };
                    inputControl = checkBox;
                }
                else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
                {
                    var textBox = new TextBox
                    {
                        Text = value?.ToString(),
                        Margin = ElementMargin,
                        Width = this.Width,
                        Style = (Style)TryFindResource("GenericViewerTextBox"),
                        Tag = prop.Name
                    };
                    textBox.TextChanged += (s, e) =>
                    {
                        if (int.TryParse(textBox.Text, out int result))
                        {
                            var old = prop.GetValue(_buildingContent);
                            prop.SetValue(_buildingContent, result);
                            RaisePropertyChanged(prop, old, result);
                        }
                    };
                    inputControl = textBox;
                }
                else if (prop.PropertyType.GetInterface("IConfig") != null)
                {
                    var t = prop.PropertyType;
                    var listProp = typeof(ModConfig).GetProperties().FirstOrDefault(p => p.PropertyType.IsGenericType &&
                        (p.PropertyType.GetGenericTypeDefinition() == typeof(List<>) ||
                        p.PropertyType.GetGenericTypeDefinition() == typeof(ObservableCollection<>)) &&
                        p.PropertyType.GetGenericArguments()[0] == t);
                    if (listProp != null)
                    {
                        var list = (IEnumerable)listProp.GetValue(ModDataStorage.Mod);
                        var currentValue = prop.GetValue(_buildingContent);
                        object selected = null;
                        if (currentValue != null)
                        {
                            foreach (var item in list)
                            {
                                if (item.Equals(currentValue))
                                {
                                    selected = currentValue;
                                    break;
                                }
                            }
                        }
                        var searchCm = new SearchableComboBox()
                        {
                            ItemsSource = list,
                            SelectedItem = selected
                        };
                        inputControl = searchCm;
                        searchCm.SelectionChanged += (s, e) =>
                        {
                            var old = prop.GetValue(_buildingContent);
                            var newValue = searchCm.SelectedItem;
                            prop.SetValue(_buildingContent, newValue);
                            RaisePropertyChanged(prop, old, newValue);
                        };
                    }
                }
                else if (prop.PropertyType == typeof(System.Windows.Media.Color))
                {
                    var colorPicker = new ColorPickerDropdown
                    {
                        SelectedColor = (Color)value,
                        Margin = ElementMargin,
                    };
                    colorPicker.SelectedColorChanged += (s, e) =>
                    {
                        var old = prop.GetValue(_buildingContent);
                        var newColor = colorPicker.SelectedColor;
                        prop.SetValue(_buildingContent, newColor);
                        RaisePropertyChanged(prop, old, newColor);
                    };
                    inputControl = colorPicker;
                }
                else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                {
                    var datePicker = new CustomDatePicker
                    {
                        SelectedDate = (DateTime?)value,
                        Margin = ElementMargin,
                        Width = this.Width
                    };
                    datePicker.SelectedDateChanged += (s, e) =>
                    {
                        var old = prop.GetValue(_buildingContent);
                        if (datePicker.SelectedDate.HasValue)
                        {
                            var newDate = datePicker.SelectedDate.Value;
                            prop.SetValue(_buildingContent, newDate);
                            RaisePropertyChanged(prop, old, newDate);
                        }
                        else
                        {
                            prop.SetValue(_buildingContent, null);
                            RaisePropertyChanged(prop, old, null);
                        }
                    };
                    inputControl = datePicker;
                }
                else if (prop.PropertyType == typeof(double) || prop.PropertyType == typeof(double?))
                {
                    var textBox = new TextBox
                    {
                        Text = value?.ToString(),
                        Margin = ElementMargin,
                        Width = this.Width,
                        Style = (Style)TryFindResource("GenericViewerTextBox"),
                        Tag = prop.Name
                    };
                    textBox.TextChanged += (s, e) =>
                    {
                        if (double.TryParse(textBox.Text, out double result))
                        {
                            var old = prop.GetValue(_buildingContent);
                            prop.SetValue(_buildingContent, result);
                            RaisePropertyChanged(prop, old, result);
                        }
                    };
                    inputControl = textBox;
                }
                else if (prop.PropertyType == typeof(float) || prop.PropertyType == typeof(float?))
                {
                    var textBox = new TextBox
                    {
                        Text = value?.ToString(),
                        Margin = ElementMargin,
                        Width = this.Width,
                        Style = (Style)TryFindResource("GenericViewerTextBox"),
                        Tag = prop.Name
                    };
                    textBox.TextChanged += (s, e) =>
                    {
                        if (float.TryParse(textBox.Text, out float result))
                        {
                            var old = prop.GetValue(_buildingContent);
                            prop.SetValue(_buildingContent, result);
                            RaisePropertyChanged(prop, old, result);
                        }
                    };
                    inputControl = textBox;
                }
                else if (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?))
                {
                    var textBox = new TextBox
                    {
                        Text = value?.ToString(),
                        Margin = ElementMargin,
                        Width = this.Width,
                        Style = (Style)TryFindResource("GenericViewerTextBox"),
                        Tag = prop.Name
                    };
                    textBox.TextChanged += (s, e) =>
                    {
                        if (decimal.TryParse(textBox.Text, out decimal result))
                        {
                            var old = prop.GetValue(_buildingContent);
                            prop.SetValue(_buildingContent, result);
                            RaisePropertyChanged(prop, old, result);
                        }
                    };
                    inputControl = textBox;
                }
                else if (prop.PropertyType == typeof(DateOnly) || prop.PropertyType == typeof(DateOnly?))
                {
                    var currentDate = (DateOnly?)value;
                    var datePicker = new CustomDatePicker
                    {
                        SelectedDate = currentDate.HasValue ? currentDate.Value.ToDateTime(new TimeOnly(0)) : null,
                        Margin = ElementMargin,
                        Width = this.Width,
                    };
                    datePicker.SelectedDateChanged += (s, e) =>
                    {
                        var old = prop.GetValue(_buildingContent);
                        if (datePicker.SelectedDate.HasValue)
                        {
                            var dateOnly = DateOnly.FromDateTime(datePicker.SelectedDate.Value);
                            prop.SetValue(_buildingContent, dateOnly);
                            RaisePropertyChanged(prop, old, dateOnly);
                        }
                        else
                        {
                            prop.SetValue(_buildingContent, null);
                            RaisePropertyChanged(prop, old, null);
                        }
                    };
                    inputControl = datePicker;
                }
                else if (prop.PropertyType == typeof(object))
                {
                    var currentValue = prop.GetValue(_buildingContent);
                    var textBox = new TextBox
                    {
                        Text = currentValue?.ToString() ?? string.Empty,
                        Margin = ElementMargin,
                        Width = this.Width,
                        Style = (Style)TryFindResource("GenericViewerTextBox"),
                        Tag = prop.Name
                    };

                    textBox.TextChanged += (s, e) =>
                    {
                        var oldValue = prop.GetValue(_buildingContent);
                        string newText = textBox.Text;

                        object newValue = newText;  // по умолчанию просто строка

                        // Если текст выглядит как число — пытаемся привести
                        if (int.TryParse(newText, out int intVal))
                        {
                            newValue = intVal;
                        }
                        else if (double.TryParse(newText, out double doubleVal))
                        {
                            newValue = doubleVal;
                        }
                        else if (bool.TryParse(newText, out bool boolVal))
                        {
                            newValue = boolVal;
                        }
                        // можно добавить другие примитивы, если нужно (DateTime, Guid и т.д.)

                        prop.SetValue(_buildingContent, newValue);
                        RaisePropertyChanged(prop, oldValue, newValue);
                    };

                    inputControl = textBox;
                }
                else if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var itemType = prop.PropertyType.GetGenericArguments()[0];
                    var list = prop.GetValue(_buildingContent) as IList;
                    if (list == null)
                    {
                        var listType = typeof(List<>).MakeGenericType(itemType);
                        list = (IList)Activator.CreateInstance(listType);
                        prop.SetValue(_buildingContent, list);
                    }

                    if (itemType == typeof(Var))
                    {
                        var sb = new StringBuilder();
                        foreach (Var item in list)
                        {
                            if (item == null || string.IsNullOrWhiteSpace(item.Name)) continue;
                            var valStr = item.Value?.ToString();
                            if (valStr != null)
                                valStr = valStr.Replace("\r", "").Replace("\n", " ");
                            sb.AppendLine($"{item.Name} = {valStr}");
                        }
                        var textBox = new TextBox
                        {
                            Text = sb.ToString(),
                            AcceptsReturn = true,
                            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                            Margin = ElementMargin,
                            Height = 100,
                            TextWrapping = TextWrapping.Wrap,
                            Width = this.Width,
                            Style = (Style)TryFindResource("GenericViewerMultilineTextBox"),
                            Tag = prop.Name
                        };
                        textBox.TextChanged += (s, e) =>
                        {
                            var newList = new List<Var>();
                            var lines = textBox.Text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var line in lines)
                            {
                                var trimmedLine = line.Trim();
                                if (string.IsNullOrWhiteSpace(trimmedLine)) continue;
                                var eqIndex = trimmedLine.IndexOf('=');
                                if (eqIndex <= 0) continue;
                                var namePart = trimmedLine.Substring(0, eqIndex).Trim();
                                var valuePart = eqIndex < trimmedLine.Length - 1
                                    ? trimmedLine.Substring(eqIndex + 1).Trim()
                                    : null;
                                if (string.IsNullOrWhiteSpace(namePart)) continue;
                                newList.Add(new Var
                                {
                                    Name = namePart,
                                    Value = !string.IsNullOrWhiteSpace(valuePart) ? valuePart : null
                                });
                            }
                            var oldList = prop.GetValue(_buildingContent);
                            prop.SetValue(_buildingContent, newList);
                            RaisePropertyChanged(prop, oldList, newList);
                        };
                        inputControl = textBox;
                    }
                    else if (itemType.IsEnum || itemType == typeof(string) || itemType == typeof(int) || itemType == typeof(int?))
                    {
                        var expander = new Expander
                        {
                            Header = $"{prop.Name} ({list.Count} items)",
                            IsExpanded = false,
                            Margin = ElementMargin,
                            Width = this.Width,
                            Style = (Style)TryFindResource("GenericViewerExpander"),
                        };
                        var itemsPanel = new StackPanel { Orientation = Orientation.Vertical };
                        foreach (var item in list)
                        {
                            var itemControl = CreateControlForType(itemType, item, (newItem) =>
                            {
                                var index = list.IndexOf(item);
                                if (index >= 0)
                                {
                                    list[index] = newItem;
                                    RaisePropertyChanged(prop, item, newItem);
                                }
                            });
                            if (itemControl != null)
                            {
                                var removeButton = new Button
                                {
                                    Content = "Remove",
                                    Margin = new Thickness(0, 2, 0, 2),
                                    Style = (Style)TryFindResource("GenericViewerButton")
                                };
                                removeButton.Click += (s, e) =>
                                {
                                    list.Remove(item);
                                    itemsPanel.Children.Remove(itemControl.Parent as UIElement);
                                    expander.Header = $"{prop.Name} ({list.Count} items)";
                                    RaisePropertyChanged(prop, list, list);
                                };
                                var itemContainer = new StackPanel { Orientation = Orientation.Horizontal };
                                itemContainer.Children.Add(itemControl);
                                itemContainer.Children.Add(removeButton);
                                itemsPanel.Children.Add(itemContainer);
                            }
                        }
                        var addButton = new Button
                        {
                            Content = "Add",
                            Margin = new Thickness(0, 5, 0, 0),
                            Style = (Style)TryFindResource("GenericViewerButton")
                        };
                        addButton.Click += (s, e) =>
                        {
                            object newItem = null;
                            if (itemType.IsEnum)
                            {
                                var enumValues = Enum.GetValues(itemType);
                                newItem = enumValues.Length > 0 ? enumValues.GetValue(0) : null;
                            }
                            else if (itemType == typeof(string))
                            {
                                newItem = string.Empty;
                            }
                            else if (itemType == typeof(int) || itemType == typeof(int?))
                            {
                                newItem = 0;
                            }
                            if (newItem != null)
                            {
                                list.Add(newItem);
                                var itemControl = CreateControlForType(itemType, newItem, (updatedItem) =>
                                {
                                    var index = list.IndexOf(newItem);
                                    if (index >= 0)
                                    {
                                        list[index] = updatedItem;
                                        RaisePropertyChanged(prop, newItem, updatedItem);
                                    }
                                });
                                if (itemControl != null)
                                {
                                    var removeButton = new Button
                                    {
                                        Content = "Remove",
                                        Margin = new Thickness(0, 2, 0, 2),
                                        Style = (Style)TryFindResource("GenericViewerButton")
                                    };
                                    removeButton.Click += (s2, e2) =>
                                    {
                                        list.Remove(newItem);
                                        itemsPanel.Children.Remove(itemControl.Parent as UIElement);
                                        expander.Header = $"{prop.Name} ({list.Count} items)";
                                        RaisePropertyChanged(prop, list, list);
                                    };
                                    var itemContainer = new StackPanel { Orientation = Orientation.Horizontal };
                                    itemContainer.Children.Add(itemControl);
                                    itemContainer.Children.Add(removeButton);
                                    itemsPanel.Children.Add(itemContainer);
                                    expander.Header = $"{prop.Name} ({list.Count} items)";
                                    RaisePropertyChanged(prop, list, list);
                                }
                            }
                        };
                        itemsPanel.Children.Add(addButton);
                        expander.Content = itemsPanel;
                        inputControl = expander;
                    }
                    else if (itemType.GetInterface("IConfig") != null)
                    {
                        var listProp = typeof(ModConfig).GetProperties().FirstOrDefault(p => p.PropertyType.IsGenericType &&
                            (p.PropertyType.GetGenericTypeDefinition() == typeof(List<>) ||
                            p.PropertyType.GetGenericTypeDefinition() == typeof(ObservableCollection<>)) &&
                            p.PropertyType.GetGenericArguments()[0] == itemType);
                        if (listProp != null)
                        {
                            var availableItems = (IEnumerable)listProp.GetValue(ModDataStorage.Mod);
                            var expander = new Expander
                            {
                                Header = $"{prop.Name} ({list.Count} items)",
                                IsExpanded = false,
                                Style = (Style)TryFindResource("GenericViewerExpander"),
                                Margin = ElementMargin,
                                Width = this.Width,
                            };
                            var itemsPanel = new StackPanel { Orientation = Orientation.Vertical };
                            foreach (var item in list)
                            {
                                var searchCm = new SearchableComboBox
                                {
                                    ItemsSource = availableItems,
                                    SelectedItem = item,
                                    Margin = new Thickness(0, 2, 0, 2),
                                    Style = (Style)TryFindResource("GenericViewerSearchableComboBox")
                                };
                                searchCm.SelectionChanged += (s, e) =>
                                {
                                    var index = list.IndexOf(item);
                                    if (index >= 0)
                                    {
                                        list[index] = searchCm.SelectedItem;
                                        RaisePropertyChanged(prop, item, searchCm.SelectedItem);
                                    }
                                };
                                var removeButton = new Button
                                {
                                    Content = "Remove",
                                    Margin = new Thickness(0, 2, 0, 2),
                                    Style = (Style)TryFindResource("GenericViewerButton")
                                };
                                removeButton.Click += (s, e) =>
                                {
                                    list.Remove(item);
                                    itemsPanel.Children.Remove(searchCm.Parent as UIElement);
                                    expander.Header = $"{prop.Name} ({list.Count} items)";
                                    RaisePropertyChanged(prop, list, list);
                                };
                                var itemContainer = new StackPanel { Orientation = Orientation.Horizontal };
                                itemContainer.Children.Add(searchCm);
                                itemContainer.Children.Add(removeButton);
                                itemsPanel.Children.Add(itemContainer);
                            }
                            var addButton = new Button
                            {
                                Content = "Add",
                                Margin = new Thickness(0, 5, 0, 0),
                                Style = (Style)TryFindResource("GenericViewerButton")
                            };
                            addButton.Click += (s, e) =>
                            {
                                if (availableItems != null && availableItems.Cast<object>().Any())
                                {
                                    var firstItem = availableItems.Cast<object>().First();
                                    list.Add(firstItem);
                                    var searchCm = new SearchableComboBox
                                    {
                                        ItemsSource = availableItems,
                                        SelectedItem = firstItem,
                                        Margin = new Thickness(0, 2, 0, 2),
                                        Style = (Style)TryFindResource("GenericViewerSearchableComboBox")
                                    };
                                    searchCm.SelectionChanged += (s2, e2) =>
                                    {
                                        var index = list.IndexOf(firstItem);
                                        if (index >= 0)
                                        {
                                            list[index] = searchCm.SelectedItem;
                                            RaisePropertyChanged(prop, firstItem, searchCm.SelectedItem);
                                        }
                                    };
                                    var removeButton = new Button
                                    {
                                        Content = "Remove",
                                        Margin = new Thickness(0, 2, 0, 2),
                                        Style = (Style)TryFindResource("GenericViewerButton")
                                    };
                                    removeButton.Click += (s2, e2) =>
                                    {
                                        list.Remove(firstItem);
                                        itemsPanel.Children.Remove(searchCm.Parent as UIElement);
                                        expander.Header = $"{prop.Name} ({list.Count} items)";
                                        RaisePropertyChanged(prop, list, list);
                                    };
                                    var itemContainer = new StackPanel { Orientation = Orientation.Horizontal };
                                    itemContainer.Children.Add(searchCm);
                                    itemContainer.Children.Add(removeButton);
                                    itemsPanel.Children.Add(itemContainer);
                                    expander.Header = $"{prop.Name} ({list.Count} items)";
                                    RaisePropertyChanged(prop, list, list);
                                }
                            };
                            itemsPanel.Children.Add(addButton);
                            expander.Content = itemsPanel;
                            inputControl = expander;
                        }
                    }
                }
                else if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    var keyType = prop.PropertyType.GetGenericArguments()[0];
                    var valueType = prop.PropertyType.GetGenericArguments()[1];
                    var dictionary = prop.GetValue(_buildingContent) as IDictionary;
                    if (dictionary == null)
                    {
                        var dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                        dictionary = (IDictionary)Activator.CreateInstance(dictType);
                        prop.SetValue(_buildingContent, dictionary);
                    }

                    var expander = new Expander
                    {
                        Header = $"{prop.Name} ({dictionary.Count} items)",
                        IsExpanded = false,
                        Margin = ElementMargin,
                        Style = (Style)TryFindResource("GenericViewerExpander"),
                    };

                    var itemsPanel = new StackPanel { Orientation = Orientation.Vertical };

                    foreach (DictionaryEntry entry in dictionary)
                    {
                        var keyControl = CreateControlForType(keyType, entry.Key, null);
                        var valueControl = CreateControlForType(valueType, entry.Value, (newValue) =>
                        {
                            dictionary[entry.Key] = newValue;
                            RaisePropertyChanged(prop, entry.Value, newValue);
                        });

                        if (keyControl != null && valueControl != null)
                        {
                            // фиксированные размеры и стили — оставляем как было
                            if (keyControl is FrameworkElement keyFe)
                            {
                                keyFe.Width = 100;
                                keyFe.Margin = new Thickness(0);
                                if (keyType == typeof(string) && keyFe is TextBox tbk)
                                {
                                    tbk.Style = (Style)TryFindResource("GenericViewerExpanderTextBox");
                                    tbk.Tag = "Key";
                                }
                            }
                            if (valueControl is FrameworkElement valueFe)
                            {
                                valueFe.Width = 100;
                                valueFe.Margin = new Thickness(10, 0, 0, 0);
                                if (valueType == typeof(string) && valueFe is TextBox tbv)
                                {
                                    tbv.Style = (Style)TryFindResource("GenericViewerExpanderTextBox");
                                    tbv.Tag = "Value";
                                }
                            }

                            var itemContainer = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
                            var removeButton = new Button
                            {
                                Content = "Remove",
                                Margin = new Thickness(5, 0, 0, 0),
                                Style = (Style)TryFindResource("GenericViewerButton")
                            };
                            removeButton.Click += (s, e) =>
                            {
                                dictionary.Remove(entry.Key);
                                itemsPanel.Children.Remove(itemContainer);
                                expander.Header = $"{prop.Name} ({dictionary.Count} items)";
                                RaisePropertyChanged(prop, dictionary, dictionary);
                            };

                            itemContainer.Children.Add(keyControl);
                            itemContainer.Children.Add(valueControl);
                            itemContainer.Children.Add(removeButton);
                            itemsPanel.Children.Add(itemContainer);
                        }
                    }

                    var addButton = new Button
                    {
                        Content = "Add",
                        Margin = new Thickness(0, 5, 0, 0),
                        Style = (Style)TryFindResource("GenericViewerButton")
                    };

                    addButton.Click += (s, e) =>
                    {
                        object newKey = null;
                        object newValue = null;

                        // ─── Ключ ───────────────────────────────────────────────────────
                        if (keyType.GetInterface("IGfx") != null || keyType.GetInterface("IConfig") != null)
                        {
                            var collectionProp = typeof(ModConfig).GetProperties().FirstOrDefault(p =>
                                p.PropertyType.IsGenericType &&
                                (p.PropertyType.GetGenericTypeDefinition() == typeof(List<>) ||
                                 p.PropertyType.GetGenericTypeDefinition() == typeof(ObservableCollection<>)) &&
                                p.PropertyType.GetGenericArguments()[0] == keyType);

                            if (collectionProp != null)
                            {
                                var available = (IEnumerable)collectionProp.GetValue(ModDataStorage.Mod);
                                if (available != null && available.Cast<object>().Any())
                                {
                                    newKey = available.Cast<object>().First();
                                }
                            }
                        }
                        else if (keyType == typeof(string))
                        {
                            newKey = string.Empty;
                        }
                        else if (keyType == typeof(int) || keyType == typeof(int?))
                        {
                            newKey = 0;
                        }
                        else if (keyType == typeof(double) || keyType == typeof(double?))
                        {
                            newKey = 0.0;
                        }
                        else if (keyType == typeof(float) || keyType == typeof(float?))
                        {
                            newKey = 0.0f;
                        }
                        else if (keyType == typeof(decimal) || keyType == typeof(decimal?))
                        {
                            newKey = 0.0m;
                        }
                        else if (keyType == typeof(bool))
                        {
                            newKey = false;
                        }
                        else if (keyType.IsEnum)
                        {
                            var vals = Enum.GetValues(keyType);
                            newKey = vals.Length > 0 ? vals.GetValue(0) : null;
                        }
                        else if (keyType == typeof(object))
                        {
                            newKey = string.Empty;  // по умолчанию пустая строка
                        }
                        else
                        {
                            itemsPanel.Children.Add(new Label
                            {
                                Content = $"Type without support (key): {keyType.FullName}",
                                Foreground = Brushes.Red
                            });
                            return;
                        }

                        // ─── Значение ───────────────────────────────────────────────────
                        if (valueType.GetInterface("IGfx") != null || valueType.GetInterface("IConfig") != null)
                        {
                            var collectionProp = typeof(ModConfig).GetProperties().FirstOrDefault(p =>
                                p.PropertyType.IsGenericType &&
                                (p.PropertyType.GetGenericTypeDefinition() == typeof(List<>) ||
                                 p.PropertyType.GetGenericTypeDefinition() == typeof(ObservableCollection<>)) &&
                                p.PropertyType.GetGenericArguments()[0] == valueType);

                            if (collectionProp != null)
                            {
                                var available = (IEnumerable)collectionProp.GetValue(ModDataStorage.Mod);
                                if (available != null && available.Cast<object>().Any())
                                {
                                    newValue = available.Cast<object>().First();
                                }
                            }
                        }
                        else if (valueType == typeof(string))
                        {
                            newValue = string.Empty;
                        }
                        else if (valueType == typeof(int) || valueType == typeof(int?))
                        {
                            newValue = 0;
                        }
                        else if (valueType == typeof(double) || valueType == typeof(double?))
                        {
                            newValue = 0.0;
                        }
                        else if (valueType == typeof(float) || valueType == typeof(float?))
                        {
                            newValue = 0.0f;
                        }
                        else if (valueType == typeof(decimal) || valueType == typeof(decimal?))
                        {
                            newValue = 0.0m;
                        }
                        else if (valueType == typeof(bool))
                        {
                            newValue = false;
                        }
                        else if (valueType.IsEnum)
                        {
                            var vals = Enum.GetValues(valueType);
                            newValue = vals.Length > 0 ? vals.GetValue(0) : null;
                        }
                        else if (valueType == typeof(object))
                        {
                            newValue = string.Empty;  // по умолчанию пустая строка
                        }
                        else
                        {
                            itemsPanel.Children.Add(new Label
                            {
                                Content = $"Type without support (value): {valueType.FullName}",
                                Foreground = Brushes.Red
                            });
                            return;
                        }

                        if (newKey != null)
                        {
                            dictionary[newKey] = newValue;

                            var keyControl = CreateControlForType(keyType, newKey, null);
                            var valueControl = CreateControlForType(valueType, newValue, (updatedValue) =>
                            {
                                dictionary[newKey] = updatedValue;
                                RaisePropertyChanged(prop, newValue, updatedValue);
                            });

                            if (keyControl != null && valueControl != null)
                            {
                                // фиксированные размеры и стили (как было)
                                if (keyControl is FrameworkElement kf) { kf.Width = 100; kf.Margin = new Thickness(0); }
                                if (valueControl is FrameworkElement vf) { vf.Width = 100; vf.Margin = new Thickness(10, 0, 0, 0); }

                                var itemContainer = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
                                var removeBtn = new Button
                                {
                                    Content = "Remove",
                                    Margin = new Thickness(5, 0, 0, 0),
                                    Style = (Style)TryFindResource("GenericViewerButton")
                                };
                                removeBtn.Click += (s2, e2) =>
                                {
                                    dictionary.Remove(newKey);
                                    itemsPanel.Children.Remove(itemContainer);
                                    expander.Header = $"{prop.Name} ({dictionary.Count} items)";
                                    RaisePropertyChanged(prop, dictionary, dictionary);
                                };

                                itemContainer.Children.Add(keyControl);
                                itemContainer.Children.Add(valueControl);
                                itemContainer.Children.Add(removeBtn);
                                itemsPanel.Children.Add(itemContainer);

                                expander.Header = $"{prop.Name} ({dictionary.Count} items)";
                                RaisePropertyChanged(prop, dictionary, dictionary);
                            }
                        }
                    };

                    itemsPanel.Children.Add(addButton);
                    expander.Content = itemsPanel;
                    inputControl = expander;
                }
                else if ((prop.PropertyType.GetInterface(nameof(IGfx)) != null ||
                         typeof(IGfx).IsAssignableFrom(prop.PropertyType)))
                {
                    // Получаем текущее значение свойства
                    var currentValue = prop.GetValue(_buildingContent);

                    // Создаём контрол через CreateControlForType
                    var gfxControl = CreateControlForType(prop.PropertyType, currentValue, (newValue) =>
                    {
                        // При изменении значения — сохраняем в объект и поднимаем событие
                        var old = prop.GetValue(_buildingContent);
                        prop.SetValue(_buildingContent, newValue);
                        RaisePropertyChanged(prop, old, newValue);
                    });

                    if (gfxControl != null)
                    {
                        inputControl = gfxControl;
                    }
                }

                if (inputControl != null)
                {
                    var label = new TextBlock
                    {
                        Text = prop.Name,
                        Style = (Style)TryFindResource("GenericViewerSectionTitle"),
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        Margin = new Thickness(ElementMargin.Left, 0, 10, 0)
                    };

                    if (inputControl is FrameworkElement fe)
                    {
                        fe.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                        fe.Margin = new Thickness(0, 0, ElementMargin.Right, 0);
                    }

                    var rowPanel = new Grid
                    {
                        Margin = new Thickness(0, ElementMargin.Top, 0, ElementMargin.Bottom)
                    };
                    rowPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                    rowPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    rowPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

                    Grid.SetColumn(label, 0);
                    Grid.SetColumn(inputControl, 2);

                    rowPanel.Children.Add(label);
                    rowPanel.Children.Add(inputControl);

                    rowPanel.HorizontalAlignment = GetHorizontalAlignment();
                    if (this.Width > 0)
                    {
                        rowPanel.Width = this.Width;
                    }

                    Children.Add(rowPanel);
                }
            }

            InvalidateMeasure();
            InvalidateArrange();
        }
        private System.Windows.HorizontalAlignment GetHorizontalAlignment()
        {
            return _elementOrientation switch
            {
                ContentOrientation.Left => System.Windows.HorizontalAlignment.Left,
                ContentOrientation.Center => System.Windows.HorizontalAlignment.Center,
                ContentOrientation.Right => System.Windows.HorizontalAlignment.Right,
                _ => System.Windows.HorizontalAlignment.Left
            };
        }

        private static Type GetNullableUnderlyingType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return type.GetGenericArguments()[0];
            }
            return null;
        }
        private void ApplyGenericViewerStyle(FrameworkElement control)
        {
            var controlTypeName = control.GetType().Name;  // Для отладки

            if (control is TextBox tb)
            {
                tb.Style = (Style)TryFindResource("GenericViewerTextBox");  // Стиль с CornerRadius=2, Margin=0,0,0,10
                tb.Width = 200;  // Фиксированный размер
            }
            else if (control is SearchableComboBox cb)
            {
                cb.Style = (Style)TryFindResource("GenericViewerSearchableComboBox");  // BasedOn="ComboBoxDark", Margin=0,0,0,10
                cb.Width = 200;
            }
            else if (control is CheckBox chb)
            {
                chb.Style = (Style)TryFindResource("GenericViewerCheckBox");  // BasedOn="CheckBoxDark", Margin=0,0,0,10
            }
            else if (control is Expander exp)
            {
                exp.Style = (Style)TryFindResource("GenericViewerExpander");  // Foreground="#ffffff", Margin=0,0,0,10 — исправление применения к Expander
                exp.Width = 200;  // Фиксированный размер для списков

                // Рекурсия для внутренних элементов списков (стилизация списков интерфейсов: SearchableComboBox, TextBox, Button)
                if (exp.Content is Panel panel)
                {
                    foreach (var child in panel.Children.OfType<FrameworkElement>())
                    {
                        ApplyGenericViewerStyle(child);  // Рекурсивно
                        if (child is Button btn)
                        {
                            btn.Style = (Style)TryFindResource("GenericViewerButton");  // BasedOn="ButtonBase", Margin=0,5,0,0
                            btn.Width = 100;
                        }
                        else if (child is TextBox innerTb)
                        {
                            innerTb.Style = (Style)TryFindResource("GenericViewerExpanderTextBox");  // Для элементов в Expander, Margin=0,2
                            innerTb.Width = 100;
                        }
                        else if (child is SearchableComboBox scb)  // Для списков IConfig/IGfx
                        {
                            scb.Style = (Style)TryFindResource("GenericViewerSearchableComboBox");  // Адаптация для SearchableComboBox
                            scb.Width = 200;
                        }
                    }
                }
            }
            else if (control is CustomDatePicker dp)
            {
                dp.Margin = new Thickness(0, 0, 0, 10);  // Как в примерах
                dp.Width = 200;
            }
            else if (control is ColorPickerDropdown cpd)
            {
                cpd.Width = 120;  // Как в примерах
                cpd.Margin = new Thickness(0, 0, 0, 10);
            }
            else if (control is LocalisationDisplay ld)
            {
                ld.Margin = new Thickness(0, 0, 0, 10);  // Как в примерах
                ld.Width = 200;
            }

            // Подробная отладка: Детали применения стиля
            Logger.AddDbgLog($"Применён стиль GenericViewer к элементу типа {controlTypeName} с Width={control.Width}.");
        }
        private FrameworkElement CreateControlForType(Type type, object value, Action<object> onValueChanged)
        {
            if (type == typeof(string))
            {
                var textBox = new TextBox
                {
                    Text = value?.ToString() ?? string.Empty,
                    Width = 200,
                    Style = (Style)TryFindResource("GenericViewerTextBox")
                };
                ApplyGenericViewerStyle(textBox);
                if (onValueChanged != null)
                {
                    textBox.TextChanged += (s, e) => onValueChanged(textBox.Text);
                }
                return textBox;
            }
            else if (type == typeof(int) || type == typeof(int?))
            {
                var textBox = new TextBox
                {
                    Text = value?.ToString() ?? "0",
                    Width = 200,
                    Style = (Style)TryFindResource("GenericViewerTextBox")
                };

                ApplyGenericViewerStyle(textBox);
                if (onValueChanged != null)
                {
                    textBox.TextChanged += (s, e) =>
                    {
                        if (int.TryParse(textBox.Text, out int result))
                        {
                            onValueChanged(result);
                        }
                    };
                }
                return textBox;
            }
            else if (type.IsEnum)
            {
                var enumValues = Enum.GetValues(type);
                var comboBox = new SearchableComboBox
                {
                    ItemsSource = enumValues,
                    SelectedItem = value,
                    Width = 200,
                    Style = (Style)TryFindResource("GenericViewerSearchableComboBox")
                };

                ApplyGenericViewerStyle(comboBox);
                if (onValueChanged != null)
                {
                    comboBox.SelectionChanged += (s, e) => onValueChanged(comboBox.SelectedItem);
                }
                return comboBox;
            }
            else if (type.GetInterface("IConfig") != null)
            {
                var listProp = typeof(ModConfig).GetProperties().FirstOrDefault(p => p.PropertyType.IsGenericType &&
                    (p.PropertyType.GetGenericTypeDefinition() == typeof(List<>) ||
                    p.PropertyType.GetGenericTypeDefinition() == typeof(ObservableCollection<>)) &&
                    p.PropertyType.GetGenericArguments()[0] == type);
                if (listProp != null)
                {
                    var availableItems = (IEnumerable)listProp.GetValue(ModDataStorage.Mod);
                    var searchCm = new SearchableComboBox
                    {
                        ItemsSource = availableItems,
                        SelectedItem = value,
                        Width = 200,
                    };

                    ApplyGenericViewerStyle(searchCm);
                    if (onValueChanged != null)
                    {
                        searchCm.SelectionChanged += (s, e) => onValueChanged(searchCm.SelectedItem);
                    }
                    return searchCm;
                }
            }
            else if (type == typeof(Identifier))
            {
                var identifier = value as Identifier;
                var textBox = new TextBox
                {
                    Text = identifier?.ToString() ?? string.Empty,
                    Width = 200,
                    Style = (Style)TryFindResource("GenericViewerTextBox")
                };

                ApplyGenericViewerStyle(textBox);
                if (onValueChanged != null)
                {
                    textBox.TextChanged += (s, e) => onValueChanged(new Identifier(textBox.Text));
                }
                return textBox;
            }
            else if (typeof(IGfx).IsAssignableFrom(type))
            {
                var currentGfx = value as IGfx;

                var rootStack = new StackPanel { Margin = new Thickness(0, 4, 0, 0) };

                // ─── Выбор существующего gfx ───────────────────────────────────
                var combo = new SearchableComboBox
                {
                    ItemsSource = ModDataStorage.Mod?.Gfxes ?? new ObservableCollection<IGfx>(),
                    SelectedItem = currentGfx,
                    Name = "GfxesComboBox",               // или "Id", "DisplayName" — подставь своё
                    Width = 240,
                    Margin = new Thickness(0, 0, 0, 8),
                    IsEnabled = true
                };

                combo.SelectionChanged += (s, e) =>
                {
                    onValueChanged?.Invoke(combo.SelectedItem);
                };

                // ─── Expander только для создания нового ───────────────────────
                var expander = new Expander
                {
                    Header = "Создать новую графику",
                    IsExpanded = false,
                    Margin = new Thickness(0, 8, 0, 0)
                };

                expander.Expanded += (s, e) => combo.IsEnabled = false;
                expander.Collapsed += (s, e) => combo.IsEnabled = true;

                // Содержимое экспандера
                var createPanel = new StackPanel { Margin = new Thickness(10, 6, 10, 10) };

                var lblName = new TextBlock
                {
                    Text = "Имя:",
                    Margin = new Thickness(0, 0, 0, 4)
                };

                var tbName = new TextBox
                {
                    Width = 220,
                    Margin = new Thickness(0, 0, 0, 12)
                };

                var lblImage = new TextBlock
                {
                    Text = "Изображение (перетащите файл или нажмите Обзор):",
                    Margin = new Thickness(0, 0, 0, 6)
                };

                // ─── Область drag-and-drop + превью ────────────────────────────
                var imageBorder = new Border
                {
                    Width = 180,
                    Height = 180,
                    Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(4),
                    Margin = new Thickness(0, 0, 0, 12),
                    AllowDrop = true
                };

                var dropImage = new Image
                {
                    Stretch = Stretch.Uniform,
                    Visibility = Visibility.Collapsed
                };

                var dropText = new TextBlock
                {
                    Text = "Перетащите изображение сюда\nили нажмите Обзор...",
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.Gray,
                    TextWrapping = TextWrapping.Wrap
                };

                var dropGrid = new Grid();
                dropGrid.Children.Add(dropText);
                dropGrid.Children.Add(dropImage);

                imageBorder.Child = dropGrid;

                // ─── Drag & Drop события ───────────────────────────────────────
                imageBorder.DragEnter += (s, e) =>
                {
                    if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    {
                        imageBorder.BorderBrush = Brushes.DodgerBlue;
                        imageBorder.Background = new SolidColorBrush(Color.FromArgb(80, 0, 120, 215));
                        e.Handled = true;
                    }
                };

                imageBorder.DragLeave += (s, e) =>
                {
                    imageBorder.BorderBrush = Brushes.Gray;
                    imageBorder.Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
                };

                imageBorder.Drop += (s, e) =>
                {
                    imageBorder.BorderBrush = Brushes.Gray;
                    imageBorder.Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));

                    if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    {
                        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                        if (files.Length > 0)
                        {
                            string filePath = files[0];
                            if (IsImageFile(filePath))
                            {
                                try
                                {
                                    dropImage.Source = new BitmapImage(new Uri(filePath));
                                    dropImage.Visibility = Visibility.Visible;
                                    dropText.Visibility = Visibility.Collapsed;
                                    imageBorder.Tag = filePath;  // сохраняем путь
                                }
                                catch
                                {
                                    CustomMessageBox.Show("Не удалось загрузить изображение", "Ошибка");
                                }
                            }
                            else
                            {
                                CustomMessageBox.Show("Поддерживаются только изображения (png, jpg, jpeg, bmp)", "Неверный формат");
                            }
                        }
                    }
                    e.Handled = true;
                };

                // ─── Кнопка Обзор ──────────────────────────────────────────────
                var btnBrowse = new Button
                {
                    Content = "Обзор...",
                    Width = 80,
                    Margin = new Thickness(0, 0, 0, 12)
                };

                btnBrowse.Click += (s, e) =>
                {
                    var ofd = new Microsoft.Win32.OpenFileDialog
                    {
                        Filter = "Изображения (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|Все файлы|*.*"
                    };

                    if (ofd.ShowDialog() == true)
                    {
                        string filePath = ofd.FileName;
                        try
                        {
                            dropImage.Source = new BitmapImage(new Uri(filePath));
                            dropImage.Visibility = Visibility.Visible;
                            dropText.Visibility = Visibility.Collapsed;
                            imageBorder.Tag = filePath;
                        }
                        catch
                        {
                            CustomMessageBox.Show("Не удалось загрузить изображение", "Ошибка");
                        }
                    }
                };

                // ─── Кнопка создания ───────────────────────────────────────────
                var btnCreate = new Button
                {
                    Content = "Создать и выбрать",
                    Padding = new Thickness(16, 8, 16, 8),
                    Margin = new Thickness(0, 8, 0, 0),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left
                };

                btnCreate.Click += (s, e) =>
                {
                    var name = tbName.Text?.Trim();
                    var path = imageBorder.Tag as string;

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        CustomMessageBox.Show("Укажите имя", "Ошибка");
                        return;
                    }

                    if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    {
                        CustomMessageBox.Show("Выберите изображение", "Ошибка");
                        return;
                    }

                    var newGfx = new SpriteType()
                    {
                        Id = new(name),
                        Content = dropImage.Source.ToBitmap(),
                        TexturePath = DataDefaultValues.NeedToHandle,
                    };
                    if (newGfx != null)
                    {
                        ModDataStorage.Mod.Gfxes.Add(newGfx);

                        var oldSource = combo.ItemsSource;
                        combo.ItemsSource = null;
                        combo.ItemsSource = oldSource;
                        combo.SelectedItem = newGfx;

                        expander.IsExpanded = false;

                        // сброс формы
                        tbName.Text = "";
                        dropImage.Source = null;
                        dropImage.Visibility = Visibility.Collapsed;
                        dropText.Visibility = Visibility.Visible;
                        imageBorder.Tag = null;
                    }
                };

                // собираем содержимое экспандера
                createPanel.Children.Add(lblName);
                createPanel.Children.Add(tbName);
                createPanel.Children.Add(lblImage);
                createPanel.Children.Add(imageBorder);
                createPanel.Children.Add(btnBrowse);
                createPanel.Children.Add(btnCreate);

                expander.Content = createPanel;

                // ─── Итоговая сборка ───────────────────────────────────────────
                rootStack.Children.Add(new TextBlock
                {
                    Text = "Графика (IGfx)",
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 0, 0, 4)
                });
                rootStack.Children.Add(combo);
                rootStack.Children.Add(expander);

                return rootStack;
            }

            return new TextBox
            {
                Text = value?.ToString() ?? string.Empty,
                IsReadOnly = true,
                Width = 200,
                Style = (Style)TryFindResource("GenericViewerTextBox")
            };

        }
        private bool IsImageFile(string path)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant();
            return ext is ".png" or ".jpg" or ".jpeg" or ".bmp";
        }
    }
}
