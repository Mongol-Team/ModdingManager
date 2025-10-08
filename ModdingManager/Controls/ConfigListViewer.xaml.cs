
using ModdingManagerClassLib.Debugging;
using ModdingManagerClassLib.Extentions;
using ModdingManagerModels;
using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using Image = System.Windows.Controls.Image;
using Orientation = System.Windows.Controls.Orientation;
using UserControl = System.Windows.Controls.UserControl;

namespace ModdingManager.Controls
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

        private StackPanel _panel;
        private ScrollViewer _scrollViewer;

        public ConfigListViewer()
        {
            _scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            _panel = new StackPanel();
            _scrollViewer.Content = _panel;
            Content = _scrollViewer;

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
            if (d is ConfigListViewer viewer)
            {
                if (viewer._panel != null)
                {
                    viewer._panel.Orientation = viewer.Orientation;
                }
                viewer.RebuildUI();
            }
        }

        private void RebuildUI()
        {
            _panel.Children.Clear();
            _panel.Orientation = Orientation;

            if (Source == null || ElemSize <= 0)
            {
                return;
            }

            var sw = Stopwatch.StartNew();

            foreach (var config in Source)
            {
                if (config == null || config.Id == null || !config.Id.HasValue())
                {
                    continue;
                }

                var stack = new StackPanel
                {
                    Orientation = System.Windows.Controls.Orientation.Vertical,
                    Margin = new Thickness(5)
                };

                if (config.Gfx != null && config.Gfx.Content != null)
                {
                    var viewbox = new Viewbox
                    {
                        Width = ElemSize * 0.6,
                        Height = ElemSize * 0.6,
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

                var textBlock = new TextBlock
                {
                    Text = config.Id.ToString(),
                    FontSize = ElemSize / 3.0,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };
                stack.Children.Add(textBlock);

                var button = new Button
                {
                    Content = stack,
                    Padding = new Thickness(0),
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0)
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

                _panel.Children.Add(button);

                Logger.AddDbgLog($"Added item with Id: {config.Id.ToString()}");
            }

            sw.Stop();
            Logger.AddLog($"Rendered {Source.Count} items in {sw.ElapsedMilliseconds} ms");
        }
    }
}