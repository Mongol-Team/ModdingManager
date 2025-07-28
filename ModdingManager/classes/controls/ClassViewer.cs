using ModdingManager.classes.args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;
using CheckBox = System.Windows.Controls.CheckBox;
using Label = System.Windows.Controls.Label;
using TextBox = System.Windows.Controls.TextBox;

namespace ModdingManager.classes.controls
{
    public class ClassViewer : StackPanel
    {
        private object _buildingContent;
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

        public ClassViewer()
        {
            Orientation = System.Windows.Controls.Orientation.Vertical;
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
                var propName = prop.Name;

              
                FrameworkElement element = null;
                if (prop.PropertyType == typeof(string))
                {
                    var textBox = new TextBox
                    {
                        Text = value?.ToString(),
                        Margin = ElementMargin
                    };
                    textBox.TextChanged += (s, e) =>
                    {
                        var old = prop.GetValue(_buildingContent);
                        var newText = textBox.Text;
                        prop.SetValue(_buildingContent, newText);
                        OnPropertyChange?.Invoke(this, new PropertyChangedEventArg(prop.Name, old, newText));
                    };
                    textBox.TextChanged += (s, e) => prop.SetValue(_buildingContent, textBox.Text);
                    inputControl = textBox;
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    var checkBox = new CheckBox
                    {
                        IsChecked = (bool?)value,
                        Content = prop.Name,
                        Margin = ElementMargin
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
                        Margin = ElementMargin
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

                else if (prop.PropertyType == typeof(System.Windows.Media.Color))
                {
                    var colorPicker = new ColorPicker
                    {
                        SelectedColor = (System.Windows.Media.Color?)value,
                        Margin = ElementMargin
                    };
                    colorPicker.SelectedColorChanged += (s, e) =>
                    {
                        var old = prop.GetValue(_buildingContent);
                        if (colorPicker.SelectedColor.HasValue)
                        {
                            var newColor = colorPicker.SelectedColor.Value;
                            prop.SetValue(_buildingContent, newColor);
                            RaisePropertyChanged(prop, old, newColor);
                        }
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
                        Margin = ElementMargin
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
                        Margin = ElementMargin
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
                        Margin = ElementMargin
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
                        Margin = ElementMargin
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
                        Margin = ElementMargin
                    };
                    Children.Add(label);
                    Children.Add(inputControl);
                }
            }
        }

    }
}
