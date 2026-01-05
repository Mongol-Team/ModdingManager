using Application;
using ModdingManager.classes.utils;
using Models.Args;
using Models.Interfaces;
using Models.Types.ObjectCacheData;
using System.Collections;
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
                else if (prop.PropertyType.GetInterface("IConfig") != null && prop.PropertyType != typeof(IGfx))
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
                    var datePicker = new DatePicker
                    {
                        SelectedDate = (DateTime?)value,
                        Margin = ElementMargin
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
                    var datePicker = new DatePicker
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
                    };
                    inputControl = datePicker;
                }

                if (inputControl != null)
                {
                    var label = new Label
                    {
                        Content = prop.Name,
                        FontSize = FontSize,
                        Margin = ElementMargin,
                        Width = this.Width,
                        HorizontalAlignment = GetHorizontalAlignment()
                    };

                    inputControl.HorizontalAlignment = GetHorizontalAlignment();

                    Children.Add(label);
                    Children.Add(inputControl);
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
    }
}
