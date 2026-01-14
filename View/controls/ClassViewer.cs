using Application;
using ModdingManager.classes.utils;
using Models.Args;
using Models.Configs;
using Models.Enums;
using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.ObjectCacheData;
using Models.Types.Utils;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using CheckBox = System.Windows.Controls.CheckBox;
using Color = System.Windows.Media.Color;
using Label = System.Windows.Controls.Label;
using Size = System.Windows.Size;
using TextBox = System.Windows.Controls.TextBox;
using ComboBox = System.Windows.Controls.ComboBox;
using Button = System.Windows.Controls.Button;
using Orientation = System.Windows.Controls.Orientation;

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

        protected override Size MeasureOverride(Size constraint)
        {
            double availableWidth = _isWidthExplicitlySet
                ? Width
                : (Parent is FrameworkElement parent ? parent.ActualWidth : 200);

            if (!_isHeightExplicitlySet)
            {
                base.MeasureOverride(new Size(availableWidth, double.PositiveInfinity));

                double totalHeight = 0;
                foreach (UIElement child in Children)
                {
                    child.Measure(new Size(availableWidth, double.PositiveInfinity));
                    double marginTop = 0, marginBottom = 0;
                    if (child is FrameworkElement fe)
                    {
                        marginTop = fe.Margin.Top;
                        marginBottom = fe.Margin.Bottom;
                    }
                    totalHeight += child.DesiredSize.Height + marginTop + marginBottom;
                }

                return new Size(availableWidth, totalHeight);
            }

            return base.MeasureOverride(constraint);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            if (!_isHeightExplicitlySet)
            {
                double y = 0;
                foreach (UIElement child in Children)
                {
                    double childHeight = child.DesiredSize.Height;
                    double marginTop = 0, marginBottom = 0;
                    if (child is FrameworkElement fe)
                    {
                        marginTop = fe.Margin.Top;
                        marginBottom = fe.Margin.Bottom;
                    }

                    double x = 0;
                    if (child is FrameworkElement element)
                    {
                        switch (element.HorizontalAlignment)
                        {
                            case System.Windows.HorizontalAlignment.Center:
                                x = (arrangeBounds.Width - child.DesiredSize.Width) / 2;
                                break;
                            case System.Windows.HorizontalAlignment.Right:
                                x = arrangeBounds.Width - child.DesiredSize.Width;
                                break;
                            case System.Windows.HorizontalAlignment.Stretch:
                            case System.Windows.HorizontalAlignment.Left:
                            default:
                                x = 0;
                                break;
                        }
                    }

                    child.Arrange(new Rect(x, y + marginTop, child.DesiredSize.Width, childHeight));
                    y += childHeight + marginTop + marginBottom;
                }
                return new Size(arrangeBounds.Width, y);
            }

            return base.ArrangeOverride(arrangeBounds);
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
                        Style = (Style)TryFindResource("TextBoxDark"),
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
                        Style = (Style)TryFindResource("TextBoxDark"),
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
                        Style = (Style)TryFindResource("TextBoxDark"),
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
                    var comboBox = new ComboBox
                    {
                        ItemsSource = enumValues,
                        SelectedItem = value,
                        Margin = ElementMargin,
                        Width = this.Width,
                        Style = (Style)TryFindResource("ComboBoxDark")
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
                    
                    var languageComboBox = new ComboBox
                    {
                        ItemsSource = Enum.GetValues(typeof(Models.Enums.Language)).Cast<Models.Enums.Language>(),
                        SelectedItem = localisation.Language,
                        Margin = ElementMargin,
                        Width = this.Width,
                        Style = (Style)TryFindResource("ComboBoxDark")
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
                        Style = (Style)TryFindResource("TextSmall"),
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
                        Style = (Style)TryFindResource("TextSmall"),
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
                        Style = (Style)TryFindResource("TextBoxDark"),
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
                        Style = (Style)TryFindResource("TextBoxDark"),
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
                        p.PropertyType.GetGenericTypeDefinition() == typeof(List<>) &&
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
                        Style = (Style)TryFindResource("TextBoxDark"),
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
                        Style = (Style)TryFindResource("TextBoxDark"),
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
                        Style = (Style)TryFindResource("TextBoxDark"),
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
                            Style = (Style)TryFindResource("TextBoxDark"),
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
                            p.PropertyType.GetGenericTypeDefinition() == typeof(List<>) &&
                            p.PropertyType.GetGenericArguments()[0] == itemType);
                        if (listProp != null)
                        {
                            var availableItems = (IEnumerable)listProp.GetValue(ModDataStorage.Mod);
                            var expander = new Expander
                            {
                                Header = $"{prop.Name} ({list.Count} items)",
                                IsExpanded = false,
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
                        Width = this.Width,
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
                            var itemContainer = new StackPanel { Orientation = Orientation.Horizontal };
                            itemContainer.Children.Add(new Label { Content = "Key:", Margin = new Thickness(0, 2, 5, 2) });
                            itemContainer.Children.Add(keyControl);
                            itemContainer.Children.Add(new Label { Content = "Value:", Margin = new Thickness(10, 2, 5, 2) });
                            itemContainer.Children.Add(valueControl);
                            
                            var removeButton = new Button
                            {
                                Content = "Remove",
                                Margin = new Thickness(5, 2, 0, 2),
                            };
                            removeButton.Click += (s, e) =>
                            {
                                dictionary.Remove(entry.Key);
                                itemsPanel.Children.Remove(itemContainer);
                                expander.Header = $"{prop.Name} ({dictionary.Count} items)";
                                RaisePropertyChanged(prop, dictionary, dictionary);
                            };
                            itemContainer.Children.Add(removeButton);
                            itemsPanel.Children.Add(itemContainer);
                        }
                    }
                    
                    var addButton = new Button
                    {
                        Content = "Add",
                        Margin = new Thickness(0, 5, 0, 0),
                    };
                    addButton.Click += (s, e) =>
                    {
                        object newKey = null;
                        object newValue = null;
                        
                        if (keyType.GetInterface("IConfig") != null)
                        {
                            var listProp = typeof(ModConfig).GetProperties().FirstOrDefault(p => p.PropertyType.IsGenericType &&
                                p.PropertyType.GetGenericTypeDefinition() == typeof(List<>) &&
                                p.PropertyType.GetGenericArguments()[0] == keyType);
                            if (listProp != null)
                            {
                                var availableKeys = (IEnumerable)listProp.GetValue(ModDataStorage.Mod);
                                if (availableKeys != null && availableKeys.Cast<object>().Any())
                                {
                                    newKey = availableKeys.Cast<object>().First();
                                }
                            }
                        }
                        
                        if (valueType == typeof(object))
                        {
                            newValue = null;
                        }
                        else if (valueType == typeof(int) || valueType == typeof(int?))
                        {
                            newValue = 0;
                        }
                        else if (valueType == typeof(string))
                        {
                            newValue = string.Empty;
                        }
                        else if (valueType.IsEnum)
                        {
                            var enumValues = Enum.GetValues(valueType);
                            newValue = enumValues.Length > 0 ? enumValues.GetValue(0) : null;
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
                                var itemContainer = new StackPanel { Orientation = Orientation.Horizontal };
                                itemContainer.Children.Add(new Label { Content = "Key:", Margin = new Thickness(0, 2, 5, 2) });
                                itemContainer.Children.Add(keyControl);
                                itemContainer.Children.Add(new Label { Content = "Value:", Margin = new Thickness(10, 2, 5, 2) });
                                itemContainer.Children.Add(valueControl);
                                
                                var removeButton = new Button
                                {
                                    Content = "Remove",
                                    Margin = new Thickness(5, 2, 0, 2),
                                };
                                removeButton.Click += (s2, e2) =>
                                {
                                    dictionary.Remove(newKey);
                                    itemsPanel.Children.Remove(itemContainer);
                                    expander.Header = $"{prop.Name} ({dictionary.Count} items)";
                                    RaisePropertyChanged(prop, dictionary, dictionary);
                                };
                                itemContainer.Children.Add(removeButton);
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
                else if (prop.PropertyType.GetInterface("IGfx") != null)
                {
                    var t = prop.PropertyType;
                    var listProp = typeof(ModConfig).GetProperties().FirstOrDefault(p => p.PropertyType.IsGenericType &&
                        p.PropertyType.GetGenericTypeDefinition() == typeof(List<>) &&
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

                if (inputControl != null)
                {
                    var label = new TextBlock
                    {
                        Text = prop.Name,
                        Style = (Style)TryFindResource("TextSmall"),
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

        private FrameworkElement CreateControlForType(Type type, object value, Action<object> onValueChanged)
        {
            if (type == typeof(string))
            {
                var textBox = new TextBox
                {
                    Text = value?.ToString() ?? string.Empty,
                    Width = 200,
                    Style = (Style)FindResource("TextBoxDark")
                };
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
                    Style = (Style)FindResource("TextBoxDark")
                };
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
                var comboBox = new ComboBox
                {
                    ItemsSource = enumValues,
                    SelectedItem = value,
                    Width = 200,
                    Style = (Style)FindResource("ComboBoxDark")
                };
                if (onValueChanged != null)
                {
                    comboBox.SelectionChanged += (s, e) => onValueChanged(comboBox.SelectedItem);
                }
                return comboBox;
            }
            else if (type.GetInterface("IConfig") != null)
            {
                var listProp = typeof(ModConfig).GetProperties().FirstOrDefault(p => p.PropertyType.IsGenericType &&
                    p.PropertyType.GetGenericTypeDefinition() == typeof(List<>) &&
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
                    Style = (Style)FindResource("TextBoxDark")
                };
                if (onValueChanged != null)
                {
                    textBox.TextChanged += (s, e) => onValueChanged(new Identifier(textBox.Text));
                }
                return textBox;
            }
            
            return new TextBox
            {
                Text = value?.ToString() ?? string.Empty,
                IsReadOnly = true,
                Width = 200,
                Style = (Style)FindResource("TextBoxDark")
            };
        }
    }
}
