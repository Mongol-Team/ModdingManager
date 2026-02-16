using Application.Debugging;
using Application.Extentions;
using Models.Configs;
using Models.Interfaces;
using Models.Types.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using Image = System.Windows.Controls.Image;
using Orientation = System.Windows.Controls.Orientation;
using SystemFonts = System.Windows.SystemFonts;
using TextBox = System.Windows.Controls.TextBox;
using UserControl = System.Windows.Controls.UserControl;

namespace Controls
{
    public class ConfigItemEventArgs : RoutedEventArgs
    {
        public IConfig Item { get; set; }

        public ConfigItemEventArgs(RoutedEvent routedEvent, object source, IConfig item) : base(routedEvent, source)
        {
            Item = item;
        }
    }

    public partial class ConfigListViewer : UserControl
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source), typeof(List<IConfig>), typeof(ConfigListViewer),
            new PropertyMetadata(null, OnSourceChanged));

        public static readonly DependencyProperty ElemSizeProperty = DependencyProperty.Register(
            nameof(ElemSize), typeof(int), typeof(ConfigListViewer),
            new PropertyMetadata(100, OnElemSizeChanged));

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            nameof(Orientation), typeof(Orientation), typeof(ConfigListViewer),
            new PropertyMetadata(Orientation.Horizontal, OnOrientationChanged));

        public static readonly RoutedEvent ItemClickedEvent = EventManager.RegisterRoutedEvent(
            nameof(OnItemClicked), RoutingStrategy.Bubble, typeof(EventHandler<ConfigItemEventArgs>), typeof(ConfigListViewer));

        public static readonly RoutedEvent MouseEnterEvent = EventManager.RegisterRoutedEvent(
            nameof(OnMouseEnter), RoutingStrategy.Bubble, typeof(EventHandler<ConfigItemEventArgs>), typeof(ConfigListViewer));

        public static readonly RoutedEvent MouseLossEvent = EventManager.RegisterRoutedEvent(
            nameof(OnMouseLoss), RoutingStrategy.Bubble, typeof(EventHandler<ConfigItemEventArgs>), typeof(ConfigListViewer));

        public List<IConfig> Source
        {
            get => (List<IConfig>)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public int ElemSize
        {
            get => (int)GetValue(ElemSizeProperty);
            set => SetValue(ElemSizeProperty, value);
        }

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public event EventHandler<ConfigItemEventArgs> OnItemClicked
        {
            add => AddHandler(ItemClickedEvent, value);
            remove => RemoveHandler(ItemClickedEvent, value);
        }

        public event EventHandler<ConfigItemEventArgs> OnMouseEnter
        {
            add => AddHandler(MouseEnterEvent, value);
            remove => RemoveHandler(MouseEnterEvent, value);
        }

        public event EventHandler<ConfigItemEventArgs> OnMouseLoss
        {
            add => AddHandler(MouseLossEvent, value);
            remove => RemoveHandler(MouseLossEvent, value);
        }

        private TextBox _searchBox;
        private StackPanel _panel;
        private ScrollViewer _scrollViewer;
        private List<Button> _allButtons = new List<Button>();

        public ConfigListViewer()
        {
            _searchBox = new TextBox
            {
                Height = 25,
                Margin = new Thickness(5)
            };
            _searchBox.TextChanged += OnSearchTextChanged;

            _scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            _panel = new StackPanel
            {
                Orientation = Orientation
            };
            _scrollViewer.Content = _panel;

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            Grid.SetRow(_searchBox, 0);
            Grid.SetRow(_scrollViewer, 1);

            grid.Children.Add(_searchBox);
            grid.Children.Add(_scrollViewer);

            Content = grid;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            RebuildUI();
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConfigListViewer viewer)
            {
                viewer.RebuildUI();
            }
        }

        private static void OnElemSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConfigListViewer viewer)
            {
                viewer.RebuildUI();
            }
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConfigListViewer viewer && viewer._panel != null)
            {
                viewer._panel.Orientation = viewer.Orientation;
                viewer.RebuildUI();
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var filter = _searchBox.Text ?? "";
            _panel.Children.Clear();

            if (_allButtons.Count == 0)
            {
                return;
            }

            IEnumerable<Button> filtered;
            if (string.IsNullOrEmpty(filter))
            {
                filtered = _allButtons;
            }
            else
            {
                filtered = _allButtons.Where(btn =>
                {
                    if (btn.Content is not StackPanel stack)
                    {
                        return false;
                    }
                    var textBlock = stack.Children.OfType<TextBlock>().FirstOrDefault();
                    return textBlock != null && textBlock.Text.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
                });
            }

            foreach (var button in filtered)
            {
                _panel.Children.Add(button);
            }
        }

        private void RebuildUI()
        {
            _allButtons.Clear();
            _panel.Children.Clear();

            if (Source == null || ElemSize <= 0)
            {
                return;
            }

            var sw = Stopwatch.StartNew();

            var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            var fontFamily = SystemFonts.MessageFontFamily;
            var fontStyle = FontStyles.Normal;
            var fontWeight = FontWeights.Normal;
            var initialFontSize = ElemSize / 3.0;
            var borderSize = Math.Round(ElemSize / 10.0);
            var maxTextWidthLimit = ElemSize * 9.0; // 3 * (ElemSize + ElemSize * 2) = 3 * ElemSize * 3
            double maxContainerWidth = 0;

            // First pass: calculate the max container width needed
            foreach (var config in Source)
            {
                if (config == null || config.Id == null || string.IsNullOrEmpty(config.Id.ToString()))
                {
                    continue;
                }
                var text = config.Id.ToString();

                // Calculate text width at initial font size
                var formattedText = new FormattedText(
                    text,
                    System.Globalization.CultureInfo.CurrentUICulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new Typeface(fontFamily, fontStyle, fontWeight, FontStretches.Normal),
                    initialFontSize,
                    Brushes.Black,
                    dpi);

                var textWidth = formattedText.Width;
                textWidth = Math.Min(textWidth, maxTextWidthLimit);

                var requiredTextWidth = textWidth + borderSize;

                // Calculate image width
                double requiredImageWidth = 0;
                if (config.Gfx != null && config.Gfx.Content != null)
                {
                    var bitmap = config.Gfx.Content;
                    double origW = bitmap.Width;
                    double origH = bitmap.Height;
                    if (origH > 0)
                    {
                        double targetHeight = ElemSize * 2.0;
                        requiredImageWidth = targetHeight * (origW / origH);
                    }
                }

                var requiredWidth = Math.Max(requiredTextWidth, requiredImageWidth) + 10; // Add some margin
                maxContainerWidth = Math.Max(maxContainerWidth, requiredWidth);
            }

            // Second pass: build elements with fixed width
            foreach (var config in Source)
            {
                if (config == null || config.Id == null || string.IsNullOrEmpty(config.Id.ToString()))
                {
                    continue;
                }

                var text = config.Id.ToString();

                // Calculate personalized font size to fit within fixed width
                var formattedText = new FormattedText(
                    text,
                    System.Globalization.CultureInfo.CurrentUICulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new Typeface(fontFamily, fontStyle, fontWeight, FontStretches.Normal),
                    initialFontSize,
                    Brushes.Black,
                    dpi);

                var textWidth = formattedText.Width;
                double fontSize = initialFontSize;
                var availableTextWidth = maxContainerWidth - borderSize - 10; // Subtract margins
                availableTextWidth = Math.Min(availableTextWidth, maxTextWidthLimit);

                if (textWidth > availableTextWidth)
                {
                    var scale = availableTextWidth / textWidth;
                    fontSize = initialFontSize * scale;
                }

                // Build UI element
                var stack = new StackPanel
                {
                    Orientation = System.Windows.Controls.Orientation.Vertical,
                    Margin = new Thickness(5)
                };

                double targetImageWidth = 0;
                double targetImageHeight = 0;

                if (config.Gfx != null && config.Gfx.Content != null)
                {
                    var bitmap = config.Gfx.Content;
                    double origW = bitmap.Width;
                    double origH = bitmap.Height;

                    if (origH > 0)
                    {
                        targetImageHeight = ElemSize * 2.0;
                        targetImageWidth = targetImageHeight * (origW / origH);

                        var availableImageWidth = maxContainerWidth - 10; // Subtract margins
                        if (targetImageWidth > availableImageWidth)
                        {
                            targetImageWidth = availableImageWidth;
                            targetImageHeight = targetImageWidth * (origH / origW);
                        }
                    }

                    var viewbox = new Viewbox
                    {
                        Width = targetImageWidth,
                        Height = targetImageHeight,
                        Stretch = Stretch.Uniform
                    };

                    var imageSource = config.Gfx.Content.ToImageSource();
                    if (imageSource != null)
                    {
                        var image = new Image { Source = imageSource };
                        viewbox.Child = image;
                        stack.Children.Add(viewbox);
                    }
                }
                else
                {
                    Logger.AddDbgLog($"Element Gfx is null :{text}", "ConfigListViewer");
                }

                var textBlock = new TextBlock
                {
                    Text = text,
                    FontSize = fontSize,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };
                stack.Children.Add(textBlock);

                var type = config.GetType();
                var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .Where(p => !typeof(System.Collections.IEnumerable).IsAssignableFrom(p.PropertyType) || p.PropertyType == typeof(string))
                    .ToList();

                var tooltipBuilder = new System.Text.StringBuilder();
                tooltipBuilder.AppendLine($"Type: {type.Name}");
                tooltipBuilder.AppendLine("Fields:");

                foreach (var prop in properties)
                {
                    var value = prop.GetValue(config);
                    if (value == null)
                    {
                        tooltipBuilder.AppendLine($"{prop.Name}: null");
                        continue;
                    }

                    var propType = prop.PropertyType;
                    if (propType.IsPrimitive || propType == typeof(string) || propType == typeof(decimal) || propType == typeof(DateTime) || propType == typeof(DateTimeOffset))
                    {
                        tooltipBuilder.AppendLine($"{prop.Name}: {value}");
                    }
                    else
                    {
                        tooltipBuilder.AppendLine($"{prop.Name}: {propType.Name} {{");

                        var subProperties = propType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                            .Where(sp => !typeof(System.Collections.IEnumerable).IsAssignableFrom(sp.PropertyType) || sp.PropertyType == typeof(string))
                            .ToList();

                        foreach (var subProp in subProperties)
                        {
                            var subValue = subProp.GetValue(value);
                            tooltipBuilder.AppendLine($"  {subProp.Name}: {subValue?.ToString() ?? "null"}");
                        }

                        tooltipBuilder.AppendLine("}");
                    }
                }

                var tooltipText = tooltipBuilder.ToString();

                // Then proceed with button creation, adding button.ToolTip = tooltipText;
                var button = new Button
                {
                    Content = stack,
                    Width = maxContainerWidth,
                    Padding = new Thickness(0),
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    ToolTip = tooltipText
                };

                button.Click += (sender, e) =>
                {
                    RaiseEvent(new ConfigItemEventArgs(ItemClickedEvent, this, config));
                };

                button.MouseEnter += (sender, e) =>
                {
                    RaiseEvent(new ConfigItemEventArgs(MouseEnterEvent, this, config));
                };

                button.MouseLeave += (sender, e) =>
                {
                    RaiseEvent(new ConfigItemEventArgs(MouseLossEvent, this, config));
                };

                _allButtons.Add(button);

                Logger.AddDbgLog($"Added item with Id: {text}");
            }

            sw.Stop();
            Logger.AddLog($"Rendered {Source.Count} items in {sw.ElapsedMilliseconds} ms");

            ApplyFilter();
        }
    }
}